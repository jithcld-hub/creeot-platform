using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    [Table("Brands")]
    public class Brand
    {
        [Key]
        public long BrandId { get; set; }

        [Required, MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(2)]
        public string CountryCode { get; set; } = "IN";

        [Required, MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? WebsiteUrl { get; set; }

        [MaxLength(50)]
        public string? TaxOrVatNumber { get; set; }

        [Required, StringLength(3)]
        public string BillingCurrency { get; set; } = "INR";

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal EscrowWalletBalance { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}