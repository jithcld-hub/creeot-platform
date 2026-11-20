using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashCreatorProfile
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("handle")]
        public string? Handle { get; set; }

        [JsonPropertyName("fullname")]
        public string? FullName { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("followers")]
        public long Followers { get; set; }

        [JsonPropertyName("engagementRate")]
        public decimal EngagementRate { get; set; }

        [JsonPropertyName("averageViews")]
        public long AverageViews { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
}
