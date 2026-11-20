using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    [Table("Niches")]
    public class Niche
    {
        [Key]
        public int NicheId { get; set; }

        [Required, MaxLength(50)]
        public string NicheCode { get; set; } = string.Empty; // e.g. "lifestyle", "tech"

        [Required, MaxLength(100)]
        public string NicheName { get; set; } = string.Empty; // e.g. "Lifestyle & Fashion"

        [MaxLength(250)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}