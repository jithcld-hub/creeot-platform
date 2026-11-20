using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashSearchResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("results")]
        public List<ModashCreatorProfile> Results { get; set; } = new();
    }
}
