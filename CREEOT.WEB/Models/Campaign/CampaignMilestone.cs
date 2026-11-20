using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    public class CampaignMilestone
    {
        [Key]
        public int MilestoneId { get; set; }

        [Required]
        public int CampaignId { get; set; }

        [Required, MaxLength(150)]
        public string MilestoneTitle { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public int OrderSequence { get; set; }

        public string? SubmissionDraftUrl { get; set; }
        public string? LivePostUrl { get; set; }

        public MilestoneStatus Status { get; set; } = MilestoneStatus.PendingFund;

        [ForeignKey("CampaignId")]
        public virtual Campaign? Campaign { get; set; }
    }
}