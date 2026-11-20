namespace CREEOT.WEB.Models
{
    public class CampaignMilestoneViewModel
    {
        public int MilestoneId { get; set; }
        public string MilestoneTitle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int OrderSequence { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
