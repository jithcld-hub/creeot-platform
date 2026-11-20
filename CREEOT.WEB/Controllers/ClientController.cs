using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CREEOT.WEB.Models;
using CREEOT.WEB.Data;

namespace CREEOT.WEB.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ClientController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Client/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Look up the brand profile from Brands table
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.UserId == user.Id);

            var campaigns = await _context.Campaigns
                .Include(c => c.Milestones)
                .Where(c => c.ClientId == user.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var transactions = await _context.EscrowTransactions
                .Include(t => t.Campaign)
                .Where(t => t.PayerUserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var pendingMilestones = campaigns
                .SelectMany(c => c.Milestones.Select(m => new { Campaign = c, Milestone = m }))
                .Where(x => x.Milestone.Status == MilestoneStatus.DraftSubmitted || x.Milestone.Status == MilestoneStatus.LivePublished)
                .Select(x => new MilestoneActionItemViewModel
                {
                    MilestoneId = x.Milestone.MilestoneId,
                    CampaignId = x.Campaign.CampaignId,
                    CampaignTitle = x.Campaign.Title,
                    MilestoneTitle = x.Milestone.MilestoneTitle,
                    Amount = x.Milestone.Amount,
                    Currency = x.Campaign.Currency,
                    SubmissionDraftUrl = x.Milestone.SubmissionDraftUrl,
                    LivePostUrl = x.Milestone.LivePostUrl,
                    Status = x.Milestone.Status
                })
                .ToList();

            var vm = new BrandDashboardViewModel
            {
                BrandName = brand?.CompanyName ?? user.FullName ?? "Brand",
                ContactPerson = user.FullName ?? "Team",
                Currency = brand?.CountryCode == "AE" ? "AED" : brand?.CountryCode == "QA" ? "QAR" : "INR",
                TotalCampaignsCount = campaigns.Count,
                ActiveCampaignsCount = campaigns.Count(c => c.Status == CampaignStatus.Active || c.Status == CampaignStatus.UnderReview),
                TotalEscrowLocked = transactions.Where(t => t.Status == TransactionStatus.LockedInEscrow).Sum(t => t.Amount),
                TotalEscrowPaidOut = transactions.Where(t => t.Status == TransactionStatus.ReleasedToCreator).Sum(t => t.Amount),
                PendingReviewMilestones = pendingMilestones,
                RecentCampaigns = campaigns.Take(5).Select(c => new CampaignOverviewItemViewModel
                {
                    CampaignId = c.CampaignId,
                    Title = c.Title,
                    Platform = c.Platform,
                    TotalBudget = c.TotalBudget,
                    Currency = c.Currency,
                    Status = c.Status,
                    CompletedMilestones = c.Milestones.Count(m => m.Status == MilestoneStatus.ApprovedAndReleased),
                    TotalMilestones = c.Milestones.Count,
                    DeadlineDate = c.DeadlineDate
                }).ToList(),
                RecentTransactions = transactions.Select(t => new EscrowTransactionListItemViewModel
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
                        _ => "Escrow Vault"
                    },
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return View(vm);
        }

        // GET: /Client/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.UserId == user.Id);

            var vm = new BrandSettingsViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                CompanyName = brand?.CompanyName ?? string.Empty,
                Industry = brand?.Industry ?? "E-Commerce",
                WebsiteUrl = brand?.WebsiteUrl,
                TaxOrVatNumber = brand?.TaxOrVatNumber,
                CountryCode = brand?.CountryCode ?? "IN",
                DefaultCurrency = (brand?.CountryCode) switch
                {
                    "AE" => "AED",
                    "QA" => "QAR",
                    _ => "INR"
                }
            };

            return View(vm);
        }

        // POST: /Client/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(BrandSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("User not found.");

            if (!ModelState.IsValid) return View(model);

            // 1. Update Identity user basic fields
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors) ModelState.AddModelError(string.Empty, err.Description);
                return View(model);
            }

            // 2. Update or create row in Brands table
            var brand = await _context.Brands.FirstOrDefaultAsync(b => b.UserId == user.Id);
            if (brand == null)
            {
                brand = new Brand
                {
                    UserId = user.Id,
                    CompanyName = model.CompanyName,
                    Industry = model.Industry,
                    WebsiteUrl = model.WebsiteUrl,
                    TaxOrVatNumber = model.TaxOrVatNumber,
                    CountryCode = model.CountryCode
                };
                _context.Brands.Add(brand);
            }
            else
            {
                brand.CompanyName = model.CompanyName;
                brand.Industry = model.Industry;
                brand.WebsiteUrl = model.WebsiteUrl;
                brand.TaxOrVatNumber = model.TaxOrVatNumber;
                brand.CountryCode = model.CountryCode;
                _context.Brands.Update(brand);
            }

            await _context.SaveChangesAsync();

            // 3. Update Password if provided
            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) && !string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var changePassResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!changePassResult.Succeeded)
                {
                    foreach (var err in changePassResult.Errors) ModelState.AddModelError(string.Empty, err.Description);
                    return View(model);
                }
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["StatusMessage"] = "Brand settings updated successfully.";
            return RedirectToAction(nameof(Settings));
        }
    }
}