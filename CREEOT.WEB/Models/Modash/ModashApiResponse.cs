using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashApiResponse
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("users")]
        public List<ModashRawUser> Users { get; set; } = new();
    }
}
