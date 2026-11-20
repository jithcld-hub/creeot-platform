using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    [Table("InfluencersPriceHistory")]
    public class InfluencerPriceHistory
    {
        [Key]
        public long PriceHistoryId { get; set; }

        public long InfluencerId { get; set; }

        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal BasePrice { get; set; }

        [Required, StringLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        [MaxLength(200)]
        public string? CalculationBasis { get; set; }

        [Required, MaxLength(100)]
        public string ApprovedBy { get; set; } = "System-Agent";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(InfluencerId))]
        public virtual Influencer Influencer { get; set; } = null!;
    }
}
