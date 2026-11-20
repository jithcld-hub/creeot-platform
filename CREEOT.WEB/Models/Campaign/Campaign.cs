using CREEOT.WEB.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    public class Campaign
    {
        [Key]
        public int CampaignId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string ClientId { get; set; } = string.Empty; // AspNetUsers FK

        public long? InfluencerId { get; set; } // Nullable until assigned or accepted

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalBudget { get; set; }

        [Required, MaxLength(10)]
        public string Currency { get; set; } = "INR"; // INR, AED, QAR

        [Required, MaxLength(50)]
        public string Platform { get; set; } = "Instagram"; // Instagram, YouTube, TikTok

        [Required, MaxLength(50)]
        public string PrimaryNiche { get; set; } = "lifestyle";

        public DateTime StartDate { get; set; }
        public DateTime DeadlineDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        // Navigation properties
        public virtual ICollection<CampaignMilestone> Milestones { get; set; } = new List<CampaignMilestone>();
    }
}