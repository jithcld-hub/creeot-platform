using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.API.Models
{
    [Table("InfluencersMetricsSnapshot")]
    public class InfluencerMetricsSnapshot
    {
        [Key]
        public long SnapshotId { get; set; }

        public long InfluencerId { get; set; }

        public DateOnly SnapshotDate { get; set; }

        [Required, MaxLength(30)]
        public string Platform { get; set; } = string.Empty;

        public long AvgViews { get; set; }
        public long FollowerCount { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? EngagementRate { get; set; }

        [Column(TypeName = "decimal(3, 2)")]
        public decimal RatingScore { get; set; }

        public int ReviewCount { get; set; }
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(InfluencerId))]
        public virtual Influencer Influencer { get; set; } = null!;
    }
}
