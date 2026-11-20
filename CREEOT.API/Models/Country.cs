using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.API.Models
{
    [Table("Countries")]
    public class Country
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        [StringLength(2)]
        [Column(TypeName = "varchar(2)")]
        public string CountryCode { get; set; } = string.Empty;

        [StringLength(10)]
        [Column(TypeName = "varchar(10)")]
        public string? PhoneCode { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;  // e.g., "USD", "INR"
        public string? CurrencySymbol { get; set; }               // e.g., "$", "₹"
        public bool IsActive { get; set; } = true;
    }
}