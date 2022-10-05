using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class IssueInfo
    {
        [JsonProperty("url"), JsonPropertyName("url")]
        public string? Url { get; set; }
        
        [JsonProperty("id"), JsonPropertyName("id")]
        public int Id { get; set; }
    }
}