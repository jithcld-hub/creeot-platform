using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class CreateCampaignViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Campaign Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brief/Scope is required")]
        [Display(Name = "Campaign Scope & Guidelines")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Target Social Platform")]
        public string Platform { get; set; } = "instagram";

        [Required]
        [Display(Name = "Niche Category")]
        public string PrimaryNiche { get; set; } = "lifestyle";

        [Required]
        [Range(100, 10000000, ErrorMessage = "Budget must be at least 100")]
        [Display(Name = "Total Escrow Budget")]
        public decimal TotalBudget { get; set; }

        [Required]
        [Display(Name = "Currency")]
        public string Currency { get; set; } = "INR";

        [Required]
        [Display(Name = "Delivery Deadline")]
        public DateTime DeadlineDate { get; set; } = DateTime.UtcNow.AddDays(14);

        public int? PreselectedInfluencerId { get; set; }
        public string? PreselectedInfluencerName { get; set; }
    }
}