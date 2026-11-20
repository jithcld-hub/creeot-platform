using System.ComponentModel.DataAnnotations;

namespace CREEOT.WEB.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please select your portal role.")]
        public UserRole Role { get; set; } = UserRole.Client;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [Display(Name = "Business or Creator Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember this device for 30 days")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}