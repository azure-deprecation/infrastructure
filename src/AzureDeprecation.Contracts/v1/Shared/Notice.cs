using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class Notice
    {
        [JsonProperty("description"), JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonProperty("links"), JsonPropertyName("links")]
        public List<string> Links { get; set; } = new();
    }
}