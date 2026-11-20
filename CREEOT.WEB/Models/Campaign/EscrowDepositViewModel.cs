using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class EscrowDepositViewModel
    {
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; } = string.Empty;
        public decimal TotalBudget { get; set; }
        public string Currency { get; set; } = "INR";
        public string Platform { get; set; } = "Instagram";
        public string PrimaryNiche { get; set; } = "lifestyle";

        public List<CampaignMilestoneViewModel> Milestones { get; set; } = new();

        [Required]
        public string PaymentMethod { get; set; } = "Card"; // "Card", "NetBanking", "UPI"

        // Card fields
        public string? CardHolderName { get; set; }
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }

        // NetBanking fields
        public string? SelectedBank { get; set; }

        // UPI fields
        public string? UpiId { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to lock funds in the Escrow vault.")]
        public bool AcceptEscrowTerms { get; set; }
    }
}