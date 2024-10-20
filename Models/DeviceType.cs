namespace ReferenceInfoSystem.Models
{
    using System.Text.Json.Serialization;

    namespace ReferenceInfoSystem.Models
    {
        public class DeviceType
        {
            [JsonPropertyName("Name")]
            public string? Name { get; set; }

            [JsonPropertyName("Description")]
            public string? Description { get; set; }

            public string? LocalName { get; set; }
        }
    }

}
