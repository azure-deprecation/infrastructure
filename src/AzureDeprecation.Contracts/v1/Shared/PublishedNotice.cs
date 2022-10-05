using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class PublishedNotice
    {
        [JsonProperty("title"), JsonPropertyName("title")]
        public string? Title { get; set; }
        
        [JsonProperty("apiInfo"), JsonPropertyName("apiInfo")]
        public ApiInfo? ApiInfo { get; set; }
        
        [JsonProperty("dashboardInfo"), JsonPropertyName("dashboardInfo")]
        public DashboardInfo? DashboardInfo { get; set; }
        
        [JsonProperty("labels"), JsonPropertyName("labels")]
        public List<string> Labels { get; set; } = new();
        
        [JsonProperty("createdAt"), JsonPropertyName("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }
        
        [JsonProperty("updatedAt"), JsonPropertyName("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }
        
        [JsonProperty("closedAt"), JsonPropertyName("closedAt")]
        public DateTimeOffset? ClosedAt { get; set; }
    }
}