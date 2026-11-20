using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.WEB.Models
{
    [Table("Platforms")]
    public class Platform
    {
        [Key]
        public int PlatformId { get; set; }

        [Required, MaxLength(30)]
        public string PlatformCode { get; set; } = string.Empty; // e.g. "instagram"

        [Required, MaxLength(50)]
        public string PlatformName { get; set; } = string.Empty; // e.g. "Instagram"

        [MaxLength(100)]
        public string? IconCssClass { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}