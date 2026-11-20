using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class SignUpViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Please choose a role.")]
        public UserRole Role { get; set; } = UserRole.Client;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Select your target country.")]
        [StringLength(2)]
        [Display(Name = "Country")]
        public string CountryCode { get; set; } = "IN";

        // ==========================================
        // Brand / Business Specific Fields
        // ==========================================
        [Display(Name = "Company / Brand Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Industry Sector")]
        public string? Industry { get; set; }

        [Display(Name = "Company Website")]
        [Url(ErrorMessage = "Enter a valid URL including https://")]
        public string? WebsiteUrl { get; set; }

        [Display(Name = "Tax / VAT / GSTIN Number (Optional)")]
        public string? TaxOrVatNumber { get; set; }

        // ==========================================
        // Influencer / Creator Specific Fields
        // ==========================================
        [Display(Name = "Social Handle (@username)")]
        public string? Handle { get; set; }

        [Display(Name = "Primary Platform")]
        public string? PrimaryPlatform { get; set; }

        [Display(Name = "Content Niche")]
        public string? PrimaryNiche { get; set; }

        // ==========================================
        // Authentication & Escrow Terms
        // ==========================================
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms of Service & Escrow Protection.")]
        public bool AgreeToTerms { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role == UserRole.Client)
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                    yield return new ValidationResult("Company / Brand Name is required for businesses.", new[] { nameof(CompanyName) });

                if (string.IsNullOrWhiteSpace(Industry))
                    yield return new ValidationResult("Please select your industry.", new[] { nameof(Industry) });
            }
            else if (Role == UserRole.Influencer)
            {
                if (string.IsNullOrWhiteSpace(Handle))
                    yield return new ValidationResult("Social handle (e.g. @username) is required.", new[] { nameof(Handle) });

                if (string.IsNullOrWhiteSpace(PrimaryPlatform))
                    yield return new ValidationResult("Please select your primary platform.", new[] { nameof(PrimaryPlatform) });

                if (string.IsNullOrWhiteSpace(PrimaryNiche))
                    yield return new ValidationResult("Please select your primary content niche.", new[] { nameof(PrimaryNiche) });
            }
        }
    }
}