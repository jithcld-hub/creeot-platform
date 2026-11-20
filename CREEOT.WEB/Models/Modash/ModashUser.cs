using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashUser
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("handle")]
        public string Handle { get; set; } = "";

        [JsonPropertyName("fullname")]
        public string Fullname { get; set; } = "";

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("avgViews")]
        public long? AvgViews { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("reviewCount")]
        public int? ReviewCount { get; set; }

        [JsonPropertyName("startingPrice")]
        public decimal? StartingPrice { get; set; }
    }
}
