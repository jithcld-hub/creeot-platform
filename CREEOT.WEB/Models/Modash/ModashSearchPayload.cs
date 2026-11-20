using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashSearchPayload
    {
        [JsonPropertyName("filter")]
        public ModashFilter Filter { get; set; } = new();

        [JsonPropertyName("sort")]
        public ModashSort? Sort { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("limit")]
        public int Limit { get; set; } = 9;
    }
}
