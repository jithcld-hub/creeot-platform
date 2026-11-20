using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class BrandSettingsViewModel
    {
        // Identity & Personal Profile
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Account Representative Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Work Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Contact Phone")]
        public string? PhoneNumber { get; set; }

        // Company & Billing Details
        [Required(ErrorMessage = "Company Name is required")]
        [Display(Name = "Registered Business / Brand Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "Primary Industry")]
        public string? Industry { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL starting with https://")]
        [Display(Name = "Official Website")]
        public string? WebsiteUrl { get; set; }

        [Display(Name = "GSTIN / TRN / Tax ID")]
        public string? TaxOrVatNumber { get; set; }

        [Display(Name = "Billing / Operating Country")]
        public string CountryCode { get; set; } = "IN";

        [Display(Name = "Default Campaign Currency")]
        public string DefaultCurrency { get; set; } = "INR";

        // Security / Change Password (Optional fields)
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New passwords do not match")]
        [Display(Name = "Confirm New Password")]
        public string? ConfirmNewPassword { get; set; }

        // Escrow Notification Preferences
        public bool NotifyOnMilestoneSubmission { get; set; } = true;
        public bool NotifyOnEscrowRelease { get; set; } = true;
    }
}