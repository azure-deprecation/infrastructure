using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class DeprecationInfo
    {
        [JsonProperty("title")]
        public string? Title { get; set; }

        [JsonProperty("timeline")]
        public List<TimeLineEntry> Timeline { get; set; } = new();

        [JsonProperty("impact")]
        public Impact? Impact { get; set; }

        [JsonProperty("notice")]
        public Notice? Notice { get; set; }

        [JsonProperty("requiredAction")]
        public string? RequiredAction { get; set; }

        [JsonProperty("contact")]
        public List<ContactEntry> Contact { get; set; } = new();

        [JsonProperty("additionalInformation")]
        public string? AdditionalInformation { get; set; }
    }
}