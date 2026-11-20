using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashRawUser
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("handle")]
        public string Handle { get; set; } = string.Empty;

        [JsonPropertyName("fullname")]
        public string? Fullname { get; set; }

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("followers")]
        public long? Followers { get; set; }

        [JsonPropertyName("avgViews")]
        public long? AvgViews { get; set; }

        [JsonPropertyName("startingPrice")]
        public decimal? StartingPrice { get; set; }
    }
}
