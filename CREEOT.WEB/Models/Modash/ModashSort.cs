using System.Text.Json.Serialization;

namespace CREEOT.WEB.Models.Modash
{
    public class ModashSort
    {
        [JsonPropertyName("field")]
        public string Field { get; set; } = "followers";

        [JsonPropertyName("order")]
        public string Order { get; set; } = "desc";
    }
}
