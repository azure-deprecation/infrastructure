using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class DeprecationInfo
    {
        [JsonProperty("title"), JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonProperty("timeline"), JsonPropertyName("timeline")]
        public List<TimeLineEntry> Timeline { get; set; } = new();

        [JsonProperty("impact"), JsonPropertyName("impact")]
        public Impact? Impact { get; set; }

        [JsonProperty("notice"), JsonPropertyName("notice")]
        public Notice? Notice { get; set; }

        [JsonProperty("requiredAction"), JsonPropertyName("requiredAction")]
        public string? RequiredAction { get; set; }

        [JsonProperty("contact"), JsonPropertyName("contact")]
        public List<ContactEntry> Contact { get; set; } = new();

        [JsonProperty("additionalInformation"), JsonPropertyName("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }
}