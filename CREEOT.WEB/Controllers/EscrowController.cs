using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CREEOT.WEB.Models;
using CREEOT.WEB.Data;

namespace CREEOT.WEB.Controllers
{
    [Authorize(Roles = "Client")]
    public class EscrowController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EscrowController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Escrow or /Escrow/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var transactions = await _context.EscrowTransactions
                .Include(t => t.Campaign)
                .Where(t => t.PayerUserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var vm = new EscrowDashboardViewModel
            {
                TotalEscrowLocked = transactions
                    .Where(t => t.Status == TransactionStatus.LockedInEscrow)
                    .Sum(t => t.Amount),
                TotalEscrowReleased = transactions
                    .Where(t => t.Status == TransactionStatus.ReleasedToCreator)
                    .Sum(t => t.Amount),
                TotalEscrowRefunded = transactions
                    .Where(t => t.Status == TransactionStatus.Refunded)
                    .Sum(t => t.Amount),
                ActiveCampaignsCount = transactions
                    .Where(t => t.Status == TransactionStatus.LockedInEscrow)
                    .Select(t => t.CampaignId)
                    .Distinct()
                    .Count(),
                Transactions = transactions.Select(t => new EscrowTransactionListItemViewModel
                {
                    TransactionId = t.TransactionId,
                    CampaignId = t.CampaignId,
                    CampaignTitle = t.Campaign?.Title ?? $"Campaign #{t.CampaignId}",
                    Amount = t.Amount,
                    Currency = t.Currency,
                    PaymentMethod = t.PaymentMethod,
                    GatewayPaymentId = t.GatewayPaymentId,
                    ReferenceDetail = t.PaymentMethod switch
                    {
                        PaymentProviderMethod.Card => !string.IsNullOrEmpty(t.CardLastFour) ? $"Card •••• {t.CardLastFour}" : "Card",
                        PaymentProviderMethod.NetBanking => !string.IsNullOrEmpty(t.NetBankingBankCode) ? $"NetBanking ({t.NetBankingBankCode})" : "Net Banking",
                        PaymentProviderMethod.UPI => !string.IsNullOrEmpty(t.UpiVirtualPaymentAddress) ? $"UPI ({t.UpiVirtualPaymentAddress})" : "UPI",
                        _ => "Online"
                    },
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        // GET: /Escrow/Deposit?campaignId=5
        [HttpGet]
        public async Task<IActionResult> Deposit(int campaignId)
        {
            var userId = _userManager.GetUserId(User);

            var campaign = await _context.Campaigns
                .Include(c => c.Milestones)
                .FirstOrDefaultAsync(c => c.CampaignId == campaignId && c.ClientId == userId);

            if (campaign == null)
            {
                TempData["StatusMessage"] = "Campaign not found.";
                return RedirectToAction("Index", "Campaigns");
            }

            if (campaign.Status == CampaignStatus.Active)
            {
                TempData["StatusMessage"] = "Funds for this campaign are already locked in Escrow.";
                return RedirectToAction("Details", "Campaigns", new { id = campaign.CampaignId });
            }

            var vm = new EscrowDepositViewModel
            {
                CampaignId = campaign.CampaignId,
                CampaignTitle = campaign.Title,
                TotalBudget = campaign.TotalBudget,
                Currency = campaign.Currency,
                Platform = campaign.Platform,
                PrimaryNiche = campaign.PrimaryNiche,
                Milestones = campaign.Milestones.OrderBy(m => m.OrderSequence).Select(m => new CampaignMilestoneViewModel
                {
                    MilestoneId = m.MilestoneId,
                    MilestoneTitle = m.MilestoneTitle,
                    Amount = m.Amount,
                    OrderSequence = m.OrderSequence,
                    Status = m.Status.ToString()
                }).ToList()
            };

            return View(vm);
        }

        // POST: /Escrow/Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(EscrowDepositViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var campaign = await _context.Campaigns
                .Include(c => c.Milestones)
                .FirstOrDefaultAsync(c => c.CampaignId == model.CampaignId && c.ClientId == userId);

            if (campaign == null) return NotFound();

            // Conditional Validation based on selected method
            if (model.PaymentMethod == "Card")
            {
                if (string.IsNullOrWhiteSpace(model.CardNumber) || model.CardNumber.Replace(" ", "").Length < 15)
                    ModelState.AddModelError("CardNumber", "Enter a valid 16-digit card number.");
                if (string.IsNullOrWhiteSpace(model.ExpiryDate))
                    ModelState.AddModelError("ExpiryDate", "Enter expiry date.");
                if (string.IsNullOrWhiteSpace(model.Cvv) || model.Cvv.Length < 3)
                    ModelState.AddModelError("Cvv", "Enter valid 3-digit CVV.");
            }
            else if (model.PaymentMethod == "NetBanking")
            {
                if (string.IsNullOrWhiteSpace(model.SelectedBank))
                    ModelState.AddModelError("SelectedBank", "Please select your bank.");
            }
            else if (model.PaymentMethod == "UPI")
            {
                if (string.IsNullOrWhiteSpace(model.UpiId) ||
       (!model.UpiId.Contains("@") && model.UpiId.Trim().Length < 10))
                {
                    ModelState.AddModelError("UpiId", "Enter a valid UPI ID (e.g. name@okhdfcbank) or 12-digit bank UTR number.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Re-hydrate readonly summary data
                model.CampaignTitle = campaign.Title;
                model.TotalBudget = campaign.TotalBudget;
                model.Currency = campaign.Currency;
                model.Platform = campaign.Platform;
                model.PrimaryNiche = campaign.PrimaryNiche;
                model.Milestones = campaign.Milestones.OrderBy(m => m.OrderSequence).Select(m => new CampaignMilestoneViewModel
                {
                    MilestoneId = m.MilestoneId,
                    MilestoneTitle = m.MilestoneTitle,
                    Amount = m.Amount,
                    OrderSequence = m.OrderSequence,
                    Status = m.Status.ToString()
                }).ToList();

                return View(model);
            }

            // 1. Record the Escrow Transaction in your Current Account holding ledger
            var transaction = new EscrowTransaction
            {
                CampaignId = campaign.CampaignId,
                PayerUserId = userId!,
                Amount = campaign.TotalBudget,
                Currency = campaign.Currency,
                PaymentMethod = model.PaymentMethod switch
                {
                    "NetBanking" => PaymentProviderMethod.NetBanking,
                    "UPI" => PaymentProviderMethod.UPI,
                    _ => PaymentProviderMethod.Card
                },
                GatewayOrderId = $"CR_ORD_{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
                GatewayPaymentId = $"CR_PAY_{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
                CardLastFour = model.PaymentMethod == "Card" && !string.IsNullOrEmpty(model.CardNumber)
                    ? model.CardNumber.Trim().Substring(Math.Max(0, model.CardNumber.Trim().Length - 4))
                    : null,
                NetBankingBankCode = model.PaymentMethod == "NetBanking" ? model.SelectedBank : null,
                UpiVirtualPaymentAddress = model.PaymentMethod == "UPI" ? model.UpiId : null,
                Status = TransactionStatus.LockedInEscrow,
                SettledAt = DateTime.UtcNow
            };

            _context.EscrowTransactions.Add(transaction);

            // 2. Lock the Campaign & Milestones
            campaign.Status = CampaignStatus.Active;
            foreach (var milestone in campaign.Milestones)
            {
                if (milestone.Status == MilestoneStatus.PendingFund)
                {
                    milestone.Status = MilestoneStatus.FundedInEscrow;
                }
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"₹{campaign.TotalBudget:N0} successfully locked in Escrow Vault for {campaign.Title}!";
            return RedirectToAction("Details", "Campaigns", new { id = campaign.CampaignId });
        }
    }
}