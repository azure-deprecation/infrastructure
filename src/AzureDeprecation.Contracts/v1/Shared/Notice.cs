using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class Notice
    {
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("links")]
        public List<string> Links { get; set; } = new();
    }
}