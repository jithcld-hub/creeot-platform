using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    [Table("BudgetRanges")]
    public class BudgetRange
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string RangeKey { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MinAmount { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? MaxAmount { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Generates a localized label dynamically using the provided currency symbol or code.
        /// </summary>
        public string GetFormattedLabel(string symbol = "₹")
        {
            if (!MinAmount.HasValue && MaxAmount.HasValue)
            {
                return $"Under {symbol}{MaxAmount.Value:N0}";
            }
            if (MinAmount.HasValue && MaxAmount.HasValue)
            {
                return $"{symbol}{MinAmount.Value:N0} – {symbol}{MaxAmount.Value:N0}";
            }
            if (MinAmount.HasValue && !MaxAmount.HasValue)
            {
                return $"{symbol}{MinAmount.Value:N0}+";
            }
            return "All Budget Range";
        }
    }
}