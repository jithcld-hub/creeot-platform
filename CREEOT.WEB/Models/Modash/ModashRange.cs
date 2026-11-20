using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashRange
    {
        [JsonPropertyName("min")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Min { get; set; }

        [JsonPropertyName("max")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Max { get; set; }
    }
}
