using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashFilter
    {
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        [JsonPropertyName("location")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Location { get; set; }

        [JsonPropertyName("topics")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Topics { get; set; }

        [JsonPropertyName("followers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ModashRange? Followers { get; set; }
    }
}
