using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class PublishedNotice
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("apiInfo")]
        public ApiInfo? ApiInfo { get; set; }

        [JsonProperty("dashboardInfo")]
        public DashboardInfo? DashboardInfo { get; set; }

        [JsonProperty("labels")]
        public List<string> Labels { get; set; } = new();

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("closedAt")]
        public DateTimeOffset? ClosedAt { get; set; }
    }
}