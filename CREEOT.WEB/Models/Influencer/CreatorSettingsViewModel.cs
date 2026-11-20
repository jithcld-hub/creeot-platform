using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class CreatorSettingsViewModel
    {
        // Identity & Personal Profile
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Display / Legal Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Account Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contact Phone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Country / Region")]
        public string CountryCode { get; set; } = "IN";

        // Creator Profile & Marketplace Specs
        [Required(ErrorMessage = "Creator handle is required")]
        [Display(Name = "Primary Social Handle")]
        public string Handle { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Primary Platform")]
        public string PrimaryPlatform { get; set; } = "instagram";

        [Required]
        [Display(Name = "Primary Niche")]
        public string PrimaryNiche { get; set; } = "lifestyle";

        [Display(Name = "Bio / Creative Focus")]
        public string? Bio { get; set; }

        [Display(Name = "Base Deliverable Rate")]
        [Range(100, 10000000, ErrorMessage = "Price must be at least 100")]
        public decimal BasePrice { get; set; } = 5000;

        // Escrow Bank & Payout Details
        [Display(Name = "Beneficiary Legal Name")]
        public string? BankAccountHolderName { get; set; }

        [Display(Name = "Bank Account / IBAN")]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "IFSC / SWIFT Code")]
        public string? BankIfscOrSwift { get; set; }

        [Display(Name = "UPI ID / VPA (For Direct India Payouts)")]
        public string? PayoutUpiId { get; set; }

        // Security / Change Password
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }
    }
}