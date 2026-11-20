namespace CREEOT.WEB.Models
{
    public class CreatorDashboardViewModel
    {
        public string CreatorName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string PrimaryPlatform { get; set; } = "Instagram";
        public string PrimaryNiche { get; set; } = "lifestyle";
        public string Currency { get; set; } = "INR";
        public decimal CurrentRating { get; set; }
        public int TotalReviews { get; set; }

        // Financial Metrics (Wallet & Escrow)
        public decimal TotalEscrowSecured { get; set; }      // Money locked for active work
        public decimal ClearedWalletBalance { get; set; }     // Money approved & ready for payout withdrawal
        public decimal LifetimeEarnings { get; set; }        // All completed payouts
        public int ActiveTasksCount { get; set; }

        // Ongoing Tasks / Milestones to fulfill
        public List<CreatorTaskItemViewModel> ActiveTasks { get; set; } = new();

        // Tasks waiting for Brand Review
        public List<CreatorTaskItemViewModel> PendingBrandReviewTasks { get; set; } = new();

        // Wallet Activity History
        public List<CreatorPayoutItemViewModel> RecentPayouts { get; set; } = new();
    }

    public class CreatorTaskItemViewModel
    {
        public int MilestoneId { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public string ClientCompanyName { get; set; } = string.Empty;
        public string MilestoneTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public DateTime DeadlineDate { get; set; }
        public string? SubmissionDraftUrl { get; set; }
        public string? LivePostUrl { get; set; }
        public MilestoneStatus Status { get; set; }
    }

    public class CreatorPayoutItemViewModel
    {
        public int TransactionId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public DateTime Date { get; set; }
        public string Method { get; set; } = "Bank Transfer";
        public string Status { get; set; } = "Cleared";
    }
}