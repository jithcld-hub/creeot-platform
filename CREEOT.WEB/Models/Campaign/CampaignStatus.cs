using CREEOT.WEB.Models;
namespace CREEOT.WEB.Models
{
    public enum CampaignStatus
    {
        Draft = 0,
        PendingEscrowDeposit = 1,
        Active = 2,
        UnderReview = 3,
        Completed = 4,
        Disputed = 5,
        Cancelled = 6
    }
}