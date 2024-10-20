using System.Text.Json;
using System.Text.Json.Serialization;

namespace ReferenceInfoSystem.Models
{
    public class Device
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }


        [JsonExtensionData]
        public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new Dictionary<string, JsonElement>();

    }
}
