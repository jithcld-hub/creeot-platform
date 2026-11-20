using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the 6-digit OTP code.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits.")]
        [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "OTP must contain numbers only.")]
        [Display(Name = "6-Digit Verification Code")]
        public string OtpCode { get; set; } = string.Empty;
    }
}