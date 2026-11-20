using CREEOT.WEB.Data;
using CREEOT.WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CREEOT.WEB.Controllers
{
    [Authorize]
    public class CampaignsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CampaignsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Campaigns/
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var isClient = User.IsInRole("Client");

            var query = _context.Campaigns
                .Include(c => c.Milestones)
                .AsQueryable();

            if (isClient)
            {
                query = query.Where(c => c.ClientId == userId);
            }
            else
            {
                // Influencer campaigns
                var influencer = await _context.Influencers.FirstOrDefaultAsync(i => i.UserId == userId);
                if (influencer != null)
                {
                    query = query.Where(c => c.InfluencerId == influencer.InfluencerId);
                }
                else
                {
                    return View(new CampaignListViewModel());
                }
            }

            var campaigns = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            var vm = new CampaignListViewModel
            {
                ActiveCampaigns = campaigns.Where(c => c.Status != CampaignStatus.Completed && c.Status != CampaignStatus.Cancelled),
                CompletedCampaigns = campaigns.Where(c => c.Status == CampaignStatus.Completed || c.Status == CampaignStatus.Cancelled)
            };

            return View(vm);
        }

        // GET: /Campaigns/Create
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Create(int? preselectedInfluencerId)
        {
            var vm = new CreateCampaignViewModel
            {
                PreselectedInfluencerId = preselectedInfluencerId
            };

            if (preselectedInfluencerId.HasValue)
            {
                var influencer = await _context.Influencers.FindAsync(preselectedInfluencerId.Value);
                if (influencer != null)
                {
                    vm.PreselectedInfluencerName = influencer.FullName;
                    vm.Platform = influencer.PrimaryPlatform;
                    vm.PrimaryNiche = influencer.PrimaryNiche;
                }
            }

            return View(vm);
        }

        // POST: /Campaigns/Create
        [HttpPost]
        [Authorize(Roles = "Client")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCampaignViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var campaign = new Campaign
            {
                Title = model.Title,
                Description = model.Description,
                ClientId = userId!,
                InfluencerId = model.PreselectedInfluencerId,
                TotalBudget = model.TotalBudget,
                Currency = model.Currency,
                Platform = model.Platform,
                PrimaryNiche = model.PrimaryNiche,
                StartDate = DateTime.UtcNow,
                DeadlineDate = model.DeadlineDate,
                Status = CampaignStatus.PendingEscrowDeposit
            };

            // Generate initial milestones (e.g., 50% draft approval, 50% post approval)
            campaign.Milestones.Add(new CampaignMilestone
            {
                MilestoneTitle = "Draft Preview & Client Script Review",
                Amount = Math.Round(model.TotalBudget * 0.40m, 2),
                OrderSequence = 1,
                Status = MilestoneStatus.PendingFund
            });

            campaign.Milestones.Add(new CampaignMilestone
            {
                MilestoneTitle = "Live Post Publication & Verification",
                Amount = Math.Round(model.TotalBudget * 0.60m, 2),
                OrderSequence = 2,
                Status = MilestoneStatus.PendingFund
            });

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = "Campaign created! Deposit funds into Escrow to activate.";
            return RedirectToAction("Details", new { id = campaign.CampaignId });
        }

        // GET: /Campaigns/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Milestones.OrderBy(m => m.OrderSequence))
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (campaign == null)
            {
                return NotFound();
            }

            return View(campaign);
        }

        // GET: /Campaigns/Assign?influencerId=3&campaignId=5
        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> Assign(long influencerId, int? campaignId)
        {
            var userId = _userManager.GetUserId(User);

            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.InfluencerId == influencerId);

            if (influencer == null)
            {
                TempData["StatusMessage"] = "Selected creator could not be found.";
                return RedirectToAction("Index", "Explore");
            }

            // Get all campaigns owned by this client that can accept assignments
            var clientCampaigns = await _context.Campaigns
                .Where(c => c.ClientId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CampaignDropdownItemViewModel
                {
                    CampaignId = c.CampaignId,
                    Title = c.Title,
                    TotalBudget = c.TotalBudget,
                    Currency = c.Currency
                })
                .ToListAsync();

            if (!clientCampaigns.Any())
            {
                TempData["StatusMessage"] = "You need to create a Campaign first before assigning creators.";
                return RedirectToAction("Create", "Campaigns", new { preselectedInfluencerId = influencerId });
            }

            var vm = new AssignWorkViewModel
            {
                InfluencerId = influencer.InfluencerId,
                InfluencerName = influencer.FullName,
                Handle = influencer.Handle,
                PrimaryPlatform = influencer.PrimaryPlatform,
                PrimaryNiche = influencer.PrimaryNiche,
                SuggestedBasePrice = influencer.CurrentBasePrice,
                Currency = influencer.CurrencyCode ?? "INR",
                CampaignId = campaignId ?? clientCampaigns.First().CampaignId,
                AgreedFee = influencer.CurrentBasePrice > 0 ? influencer.CurrentBasePrice : 5000,
                AvailableCampaigns = clientCampaigns
            };

            return View(vm);
        }

        // POST: /Campaigns/Assign
        [Authorize(Roles = "Client")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(AssignWorkViewModel model)
        {
            var userId = _userManager.GetUserId(User);

            var campaign = await _context.Campaigns
                .Include(c => c.Milestones)
                .FirstOrDefaultAsync(c => c.CampaignId == model.CampaignId && c.ClientId == userId);

            var influencer = await _context.Influencers
                .FirstOrDefaultAsync(i => i.InfluencerId == model.InfluencerId);

            if (campaign == null || influencer == null)
            {
                return NotFound();
            }

            if (model.LivePublishDueDate <= model.DraftDueDate)
            {
                ModelState.AddModelError("LivePublishDueDate", "Live post date must be after the draft deadline.");
            }

            if (!ModelState.IsValid)
            {
                // Re-hydrate read-only info
                model.InfluencerName = influencer.FullName;
                model.Handle = influencer.Handle;
                model.PrimaryPlatform = influencer.PrimaryPlatform;
                model.PrimaryNiche = influencer.PrimaryNiche;
                model.SuggestedBasePrice = influencer.CurrentBasePrice;
                model.Currency = influencer.CurrencyCode ?? "INR";
                model.AvailableCampaigns = await _context.Campaigns
                    .Where(c => c.ClientId == userId)
                    .Select(c => new CampaignDropdownItemViewModel
                    {
                        CampaignId = c.CampaignId,
                        Title = c.Title,
                        TotalBudget = c.TotalBudget,
                        Currency = c.Currency
                    }).ToListAsync();

                return View(model);
            }

            // 1. Assign influencer to the campaign
            campaign.InfluencerId = influencer.InfluencerId;

            // 2. Clear or initialize the 2-Stage Escrow Milestone Protection Schedule
            _context.CampaignMilestones.RemoveRange(campaign.Milestones);

            decimal draftAmount = Math.Round(model.AgreedFee * 0.40m, 2);
            decimal liveAmount = model.AgreedFee - draftAmount;

            var milestoneDraft = new CampaignMilestone
            {
                CampaignId = campaign.CampaignId,
                MilestoneTitle = $"Draft Review & Script Approval ({model.PrimaryPlatform.ToUpper()})",
                Amount = draftAmount,
                OrderSequence = 1,
                Status = campaign.Status == CampaignStatus.Active ? MilestoneStatus.FundedInEscrow : MilestoneStatus.PendingFund
            };

            var milestoneLive = new CampaignMilestone
            {
                CampaignId = campaign.CampaignId,
                MilestoneTitle = $"Live Publication & Verification ({model.DeliverableScope})",
                Amount = liveAmount,
                OrderSequence = 2,
                Status = campaign.Status == CampaignStatus.Active ? MilestoneStatus.FundedInEscrow : MilestoneStatus.PendingFund
            };

            _context.CampaignMilestones.Add(milestoneDraft);
            _context.CampaignMilestones.Add(milestoneLive);

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"Deliverable scope assigned to {influencer.FullName}! Next, confirm your Escrow Vault deposit.";

            // Redirect to Escrow Deposit if not funded, otherwise back to Campaign details
            if (campaign.Status == CampaignStatus.PendingEscrowDeposit || campaign.Status == CampaignStatus.Draft)
            {
                return RedirectToAction("Deposit", "Escrow", new { campaignId = campaign.CampaignId });
            }

            return RedirectToAction("Details", "Campaigns", new { id = campaign.CampaignId });
        }
    }
}