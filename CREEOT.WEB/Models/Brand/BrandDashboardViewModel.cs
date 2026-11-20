namespace CREEOT.WEB.Models
{
    public class BrandDashboardViewModel
    {
        public string BrandName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR";

        // Summary Metric Cards
        public int TotalCampaignsCount { get; set; }
        public int ActiveCampaignsCount { get; set; }
        public decimal TotalEscrowLocked { get; set; }
        public decimal TotalEscrowPaidOut { get; set; }

        // Needs Attention & Actionable Milestones
        public List<MilestoneActionItemViewModel> PendingReviewMilestones { get; set; } = new();

        // Recent Active Campaigns
        public List<CampaignOverviewItemViewModel> RecentCampaigns { get; set; } = new();

        // Activity / Escrow Ledger Feed
        public List<EscrowTransactionListItemViewModel> RecentTransactions { get; set; } = new();
    }

    public class MilestoneActionItemViewModel
    {
        public int MilestoneId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public string MilestoneTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string? SubmissionDraftUrl { get; set; }
        public string? LivePostUrl { get; set; }
        public MilestoneStatus Status { get; set; }
    }

    public class CampaignOverviewItemViewModel
    {
        public int CampaignId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Platform { get; set; } = "Instagram";
        public decimal TotalBudget { get; set; }
        public string Currency { get; set; } = "INR";
        public CampaignStatus Status { get; set; }
        public int CompletedMilestones { get; set; }
        public int TotalMilestones { get; set; }
        public DateTime DeadlineDate { get; set; }
    }
}