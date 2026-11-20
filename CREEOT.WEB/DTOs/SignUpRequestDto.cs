using System.ComponentModel.DataAnnotations;
using CREEOT.WEB.Models;

namespace CREEOT.WEB.DTOs
{
    public class SignUpRequestDto
    {
        [Required]
        public UserRole Role { get; set; } = UserRole.Client; // 1 = Client, 2 = Influencer, 3 Admin

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(2)]
        public string CountryCode { get; set; } = "IN";

        // Brand / Business Fields
        public string? CompanyName { get; set; }
        public string? Industry { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? TaxOrVatNumber { get; set; }

        // Influencer / Creator Fields
        public string? Handle { get; set; }
        public string? PrimaryPlatform { get; set; }
        public string? PrimaryNiche { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }

    public class SignUpResponseDto
    {
        public bool Succeeded { get; set; }
        public string? UserId { get; set; }
        public string? RoleName { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
