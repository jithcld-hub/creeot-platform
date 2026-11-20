using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CREEOT.WEB.Models;
using CREEOT.WEB.Data;

namespace CREEOT.WEB.Controllers
{
    [Authorize(Roles = "Influencer")]
    public class CreatorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CreatorController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: /Creator/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find Influencer profile
            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            long? influencerId = influencer?.InfluencerId;

            // Load Campaigns assigned to this influencer along with Milestones
            var campaigns = await _context.Campaigns
                .Include(c => c.Milestones)
                .Where(c => c.InfluencerId == influencerId)
                .ToListAsync();

            // Extract client users for company display names
            var clientIds = campaigns.Select(c => c.ClientId).Distinct().ToList();
            var brands = await _context.Brands
                .Where(b => clientIds.Contains(b.UserId))
                .ToDictionaryAsync(b => b.UserId, b => b.CompanyName);

            var allMilestones = campaigns
                .SelectMany(c => c.Milestones.Select(m => new {
                    Campaign = c,
                    Milestone = m,
                    ClientName = brands.ContainsKey(c.ClientId) ? brands[c.ClientId] : "Brand Client"
                }))
                .ToList();

            // Active tasks (Funded in escrow, awaiting draft or publish)
            var activeTasks = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.FundedInEscrow)
                .Select(x => new CreatorTaskItemViewModel
                {
                    MilestoneId = x.Milestone.MilestoneId,
                    CampaignId = x.Campaign.CampaignId,
                    CampaignTitle = x.Campaign.Title,
                    ClientCompanyName = x.ClientName,
                    MilestoneTitle = x.Milestone.MilestoneTitle,
                    Amount = x.Milestone.Amount,
                    Currency = x.Campaign.Currency,
                    DeadlineDate = x.Campaign.DeadlineDate,
                    SubmissionDraftUrl = x.Milestone.SubmissionDraftUrl,
                    LivePostUrl = x.Milestone.LivePostUrl,
                    Status = x.Milestone.Status
                })
                .ToList();

            // Pending client review tasks
            var pendingReviewTasks = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.DraftSubmitted || x.Milestone.Status == MilestoneStatus.LivePublished)
                .Select(x => new CreatorTaskItemViewModel
                {
                    MilestoneId = x.Milestone.MilestoneId,
                    CampaignId = x.Campaign.CampaignId,
                    CampaignTitle = x.Campaign.Title,
                    ClientCompanyName = x.ClientName,
                    MilestoneTitle = x.Milestone.MilestoneTitle,
                    Amount = x.Milestone.Amount,
                    Currency = x.Campaign.Currency,
                    DeadlineDate = x.Campaign.DeadlineDate,
                    SubmissionDraftUrl = x.Milestone.SubmissionDraftUrl,
                    LivePostUrl = x.Milestone.LivePostUrl,
                    Status = x.Milestone.Status
                })
                .ToList();

            // Financial sums
            decimal securedInEscrow = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.FundedInEscrow ||
                            x.Milestone.Status == MilestoneStatus.DraftSubmitted ||
                            x.Milestone.Status == MilestoneStatus.LivePublished)
                .Sum(x => x.Milestone.Amount);

            decimal approvedLifetime = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.ApprovedAndReleased)
                .Sum(x => x.Milestone.Amount);

            var vm = new CreatorDashboardViewModel
            {
                CreatorName = user.FullName ?? user.UserName ?? "Creator",
                Handle = influencer?.Handle ?? (user.UserName != null ? $"@{user.UserName}" : "@creator"),
                PrimaryPlatform = influencer?.PrimaryPlatform ?? "Instagram",
                PrimaryNiche = influencer?.PrimaryNiche ?? "lifestyle",
                Currency = influencer?.CurrencyCode ?? "INR",
                CurrentRating = influencer?.CurrentRating ?? 5.0m,
                TotalReviews = influencer?.CurrentReviewCount ?? 0,
                TotalEscrowSecured = securedInEscrow,
                ClearedWalletBalance = approvedLifetime,
                LifetimeEarnings = approvedLifetime,
                ActiveTasksCount = activeTasks.Count,
                ActiveTasks = activeTasks,
                PendingBrandReviewTasks = pendingReviewTasks
            };

            return View(vm);
        }

        // GET: /Creator/Settings
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var vm = new CreatorSettingsViewModel
            {
                FullName = user.FullName ?? influencer?.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? influencer?.PhoneNumber,
                CountryCode = influencer?.CountryCode ?? "IN",
                Handle = influencer?.Handle ?? $"@{user.UserName}",
                PrimaryPlatform = influencer?.PrimaryPlatform ?? "instagram",
                PrimaryNiche = influencer?.PrimaryNiche ?? "lifestyle",
                Bio = influencer?.Bio,
                BasePrice = influencer?.CurrentBasePrice ?? 5000,
                BankAccountHolderName = influencer?.BankAccountHolderName,
                BankAccountNumber = influencer?.BankAccountNumber,
                BankIfscOrSwift = influencer?.BankIfscOrSwift,
                PayoutUpiId = influencer?.PayoutUpiId
            };

            return View(vm);
        }

        // POST: /Creator/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(CreatorSettingsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Update Identity user
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            var userResult = await _userManager.UpdateAsync(user);
            if (!userResult.Succeeded)
            {
                foreach (var error in userResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // 2. Update Influencer Profile in Influencers table
            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var handle = model.Handle.Trim();
            if (!handle.StartsWith("@")) handle = $"@{handle}";

            // Calculate initials
            var parts = model.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var initials = parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : model.FullName[..Math.Min(2, model.FullName.Length)].ToUpper();

            if (influencer == null)
            {
                influencer = new Influencer
                {
                    UserId = user.Id,
                    FullName = model.FullName,
                    Initials = initials,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = model.PhoneNumber,
                    CountryCode = model.CountryCode,
                    PrimaryPlatform = model.PrimaryPlatform,
                    PrimaryNiche = model.PrimaryNiche,
                    Handle = handle,
                    Bio = model.Bio,
                    CurrentBasePrice = model.BasePrice,
                    CurrencyCode = model.CountryCode switch
                    {
                        "AE" => "AED",
                        "QA" => "QAR",
                        _ => "INR"
                    },
                    BankAccountHolderName = model.BankAccountHolderName,
                    BankAccountNumber = model.BankAccountNumber,
                    BankIfscOrSwift = model.BankIfscOrSwift,
                    PayoutUpiId = model.PayoutUpiId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Influencers.Add(influencer);
            }
            else
            {
                influencer.FullName = model.FullName;
                influencer.Initials = initials;
                influencer.PhoneNumber = model.PhoneNumber;
                influencer.CountryCode = model.CountryCode;
                influencer.PrimaryPlatform = model.PrimaryPlatform;
                influencer.PrimaryNiche = model.PrimaryNiche;
                influencer.Handle = handle;
                influencer.Bio = model.Bio;
                influencer.CurrentBasePrice = model.BasePrice;
                influencer.BankAccountHolderName = model.BankAccountHolderName;
                influencer.BankAccountNumber = model.BankAccountNumber;
                influencer.BankIfscOrSwift = model.BankIfscOrSwift;
                influencer.PayoutUpiId = model.PayoutUpiId;
                influencer.UpdatedAt = DateTime.UtcNow;
                _context.Influencers.Update(influencer);
            }

            await _context.SaveChangesAsync();

            // 3. Update Password if provided
            if (!string.IsNullOrWhiteSpace(model.CurrentPassword) && !string.IsNullOrWhiteSpace(model.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["StatusMessage"] = "Studio settings and escrow payout details saved successfully.";
            return RedirectToAction(nameof(Settings));
        }

        // POST: /Creator/SubmitMilestone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitMilestone(int milestoneId, string? draftUrl, string? liveUrl)
        {
            var milestone = await _context.CampaignMilestones
                .Include(m => m.Campaign)
                .FirstOrDefaultAsync(m => m.MilestoneId == milestoneId);

            if (milestone == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(liveUrl))
            {
                milestone.LivePostUrl = liveUrl.Trim();
                milestone.Status = MilestoneStatus.LivePublished;
                TempData["StatusMessage"] = "Live proof submitted! Brand has 48 hours to approve or payment auto-releases.";
            }
            else if (!string.IsNullOrWhiteSpace(draftUrl))
            {
                milestone.SubmissionDraftUrl = draftUrl.Trim();
                milestone.Status = MilestoneStatus.DraftSubmitted;
                TempData["StatusMessage"] = "Draft preview submitted for brand feedback.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Dashboard));
        }
        // GET: /Creator/Earnings
        [HttpGet]
        public async Task<IActionResult> Earnings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (influencer == null)
            {
                TempData["StatusMessage"] = "Please complete your creator profile setup first.";
                return RedirectToAction(nameof(Settings));
            }

            // Load campaigns & milestones for this creator
            var campaigns = await _context.Campaigns
                .Include(c => c.Milestones)
                .Where(c => c.InfluencerId == influencer.InfluencerId)
                .ToListAsync();

            var allMilestones = campaigns
                .SelectMany(c => c.Milestones.Select(m => new { Campaign = c, Milestone = m }))
                .ToList();

            // 1. Pending Escrow (Still in holding)
            decimal pendingEscrow = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.FundedInEscrow ||
                            x.Milestone.Status == MilestoneStatus.DraftSubmitted ||
                            x.Milestone.Status == MilestoneStatus.LivePublished)
                .Sum(x => x.Milestone.Amount);

            // 2. Gross Cleared Payouts (Approved milestone releases)
            decimal grossApproved = allMilestones
                .Where(x => x.Milestone.Status == MilestoneStatus.ApprovedAndReleased)
                .Sum(x => x.Milestone.Amount);

            // 3. Subtract previous withdrawals recorded in EscrowTransactions
            var pastWithdrawals = await _context.EscrowTransactions
                .Where(t => t.PayerUserId == user.Id && t.Status == TransactionStatus.ReleasedToCreator)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            decimal totalWithdrawn = pastWithdrawals.Sum(t => t.Amount);
            decimal availableBalance = Math.Max(0, grossApproved - totalWithdrawn);

            var vm = new CreatorWalletViewModel
            {
                ClearedBalance = availableBalance,
                PendingEscrowBalance = pendingEscrow,
                TotalLifetimeWithdrawn = totalWithdrawn,
                Currency = influencer.CurrencyCode ?? "INR",
                BankAccountHolderName = influencer.BankAccountHolderName,
                BankAccountNumber = influencer.BankAccountNumber,
                BankIfscOrSwift = influencer.BankIfscOrSwift,
                PayoutUpiId = influencer.PayoutUpiId,
                WithdrawAmount = availableBalance > 0 ? availableBalance : 500,
                PayoutHistory = pastWithdrawals.Select(t => new WalletPayoutHistoryItemViewModel
                {
                    PayoutReference = t.GatewayPaymentId ?? $"PAY-{t.TransactionId:D6}",
                    Amount = t.Amount,
                    Currency = t.Currency,
                    DestinationDetail = !string.IsNullOrEmpty(t.UpiVirtualPaymentAddress)
                        ? $"UPI ({t.UpiVirtualPaymentAddress})"
                        : (!string.IsNullOrEmpty(t.CardLastFour) ? $"Bank A/C •••• {t.CardLastFour}" : "Direct Bank Transfer"),
                    Status = "Completed",
                    RequestedAt = t.CreatedAt
                }).ToList(),
                ClearedDeliverables = allMilestones
                    .Where(x => x.Milestone.Status == MilestoneStatus.ApprovedAndReleased)
                    .Select(x => new ClearedMilestoneItemViewModel
                    {
                        CampaignTitle = x.Campaign.Title,
                        MilestoneTitle = x.Milestone.MilestoneTitle,
                        Amount = x.Milestone.Amount,
                        Currency = x.Campaign.Currency,
                        ReleasedDate = x.Campaign.DeadlineDate
                    }).ToList()
            };

            return View(vm);
        }

        // POST: /Creator/Withdraw
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(CreatorWalletViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (influencer == null) return NotFound();

            // Recalculate available balance directly from DB to prevent client-side tampering
            var campaigns = await _context.Campaigns
                .Include(c => c.Milestones)
                .Where(c => c.InfluencerId == influencer.InfluencerId)
                .ToListAsync();

            decimal grossApproved = campaigns
                .SelectMany(c => c.Milestones)
                .Where(m => m.Status == MilestoneStatus.ApprovedAndReleased)
                .Sum(m => m.Amount);

            decimal totalWithdrawn = await _context.EscrowTransactions
                .Where(t => t.PayerUserId == user.Id && t.Status == TransactionStatus.ReleasedToCreator)
                .SumAsync(t => t.Amount);

            decimal currentBalance = Math.Max(0, grossApproved - totalWithdrawn);

            if (model.WithdrawAmount > currentBalance)
            {
                ModelState.AddModelError("WithdrawAmount", $"Withdrawal amount exceeds your available cleared balance of {currentBalance:N0} {influencer.CurrencyCode}.");
            }

            if (model.SelectedDestination == "UPI" && string.IsNullOrWhiteSpace(influencer.PayoutUpiId))
            {
                ModelState.AddModelError("SelectedDestination", "No UPI ID found on your profile. Please add one in Studio Settings.");
            }
            else if (model.SelectedDestination == "Bank" && (string.IsNullOrWhiteSpace(influencer.BankAccountNumber) || string.IsNullOrWhiteSpace(influencer.BankIfscOrSwift)))
            {
                ModelState.AddModelError("SelectedDestination", "Incomplete bank account details. Please add your Bank Account and IFSC in Studio Settings.");
            }

            if (!ModelState.IsValid)
            {
                // Re-hydrate read-only values
                model.ClearedBalance = currentBalance;
                model.Currency = influencer.CurrencyCode ?? "INR";
                model.BankAccountHolderName = influencer.BankAccountHolderName;
                model.BankAccountNumber = influencer.BankAccountNumber;
                model.BankIfscOrSwift = influencer.BankIfscOrSwift;
                model.PayoutUpiId = influencer.PayoutUpiId;
                return View("Earnings", model);
            }

            // Log the payout debit transaction
            var payoutTx = new EscrowTransaction
            {
                CampaignId = campaigns.FirstOrDefault()?.CampaignId ?? 0,
                PayerUserId = user.Id, // Recorded against the creator recipient
                Amount = model.WithdrawAmount,
                Currency = influencer.CurrencyCode ?? "INR",
                PaymentMethod = model.SelectedDestination == "UPI" ? PaymentProviderMethod.UPI : PaymentProviderMethod.NetBanking,
                UpiVirtualPaymentAddress = model.SelectedDestination == "UPI" ? influencer.PayoutUpiId : null,
                CardLastFour = model.SelectedDestination == "Bank" && !string.IsNullOrEmpty(influencer.BankAccountNumber)
                    ? influencer.BankAccountNumber.Trim()[^Math.Min(4, influencer.BankAccountNumber.Trim().Length)..]
                    : null,
                GatewayOrderId = $"WD_ORD_{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
                GatewayPaymentId = $"WD_PAY_{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
                Status = TransactionStatus.ReleasedToCreator,
                CreatedAt = DateTime.UtcNow,
                SettledAt = DateTime.UtcNow
            };

            _context.EscrowTransactions.Add(payoutTx);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Withdrawal request for {model.WithdrawAmount:N0} {influencer.CurrencyCode} submitted successfully! Funds will credit within 2-4 hours.";
            return RedirectToAction(nameof(Earnings));
        }
    }
}