using System.Text.Json.Serialization;
using AzureDeprecation.Contracts.Enum;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class Impact
    {
        [JsonProperty("description"), JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonProperty("type"), JsonPropertyName("type")]
        public ImpactType Type { get; set; }
        
        [JsonProperty("area"), JsonPropertyName("area")]
        public ImpactArea Area { get; set; }
        
        [JsonProperty("cloud"), JsonPropertyName("cloud")]
        public AzureCloud Cloud { get; set; }
        
        [JsonProperty("services"), JsonPropertyName("services")]
        public List<AzureService> Services { get; set; } = new();
    }
}