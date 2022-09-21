using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class IssueInfo
    {
        [JsonProperty("url")]
        public string? Url { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}