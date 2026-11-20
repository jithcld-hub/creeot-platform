using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class AssignWorkViewModel
    {
        // Influencer Info
        public long InfluencerId { get; set; }
        public string InfluencerName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string PrimaryPlatform { get; set; } = "Instagram";
        public string PrimaryNiche { get; set; } = "Lifestyle";
        public decimal SuggestedBasePrice { get; set; }
        public string Currency { get; set; } = "INR";

        // Assignment Form Inputs
        [Required(ErrorMessage = "Please choose a campaign")]
        [Display(Name = "Target Campaign")]
        public int CampaignId { get; set; }

        [Required(ErrorMessage = "Deliverable scope is required")]
        [Display(Name = "Deliverable Description / Scope")]
        public string DeliverableScope { get; set; } = "1x 60s Instagram Reel + 2x Stories";

        [Required(ErrorMessage = "Agreed creator fee is required")]
        [Range(100, 10000000, ErrorMessage = "Fee must be at least 100")]
        [Display(Name = "Creator Fee / Milestone Allocation")]
        public decimal AgreedFee { get; set; }

        [Required(ErrorMessage = "Draft due date is required")]
        [Display(Name = "Draft Preview Deadline")]
        public DateTime DraftDueDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required(ErrorMessage = "Live publish deadline is required")]
        [Display(Name = "Live Post Publication Deadline")]
        public DateTime LivePublishDueDate { get; set; } = DateTime.UtcNow.AddDays(14);

        [Display(Name = "Special Creative Instructions / Brand Do's & Don'ts")]
        public string? SpecialInstructions { get; set; }

        // Dropdown Items
        public List<CampaignDropdownItemViewModel> AvailableCampaigns { get; set; } = new();
    }

    public class CampaignDropdownItemViewModel
    {
        public int CampaignId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public string Currency { get; set; } = "INR";
    }
}