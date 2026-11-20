using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CREEOT.API.Models
{
    [Table("Influencers")]
    public class Influencer
    {
        [Key]
        public long InfluencerId { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Initials { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(2)]
        public string CountryCode { get; set; } = "IN";

        [Required, MaxLength(50)]
        public string PrimaryNiche { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PrimaryPlatform { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Handle { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ProfileImageUrl { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; } = true;

        // Cached snapshot metrics for high-speed UI binding
        public long CurrentAvgViews { get; set; }
        public long CurrentFollowers { get; set; }

        [Column(TypeName = "decimal(3, 2)")]
        public decimal CurrentRating { get; set; }

        public int CurrentReviewCount { get; set; }
        public bool IsTrending { get; set; }
        public int? TrendingRank { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal CurrentBasePrice { get; set; }

        [Required, StringLength(3)]
        public string CurrencyCode { get; set; } = "INR";

        // Escrow Bank & Payout Details
        [MaxLength(150)]
        public string? BankAccountHolderName { get; set; }

        [MaxLength(100)]
        public string? BankAccountNumber { get; set; }

        [MaxLength(50)]
        public string? BankIfscOrSwift { get; set; }

        [MaxLength(100)]
        public string? PayoutUpiId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation Collections
        public virtual ICollection<InfluencerMetricsSnapshot> MetricsSnapshots { get; set; } = new List<InfluencerMetricsSnapshot>();
        public virtual ICollection<InfluencerPriceHistory> PriceHistories { get; set; } = new List<InfluencerPriceHistory>();

        // View Formatting Helpers
        [NotMapped]
        public string FormattedViews => CurrentAvgViews switch
        {
            >= 1_000_000 => $"{(CurrentAvgViews / 1_000_000.0):0.#}M",
            >= 1_000 => $"{(CurrentAvgViews / 1_000.0):0.#}K",
            _ => CurrentAvgViews.ToString()
        };

        [NotMapped]
        public string PriceString => $"{CurrentBasePrice:N0} {CurrencyCode}";

        [Required, MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser? User { get; set; }
    }
}