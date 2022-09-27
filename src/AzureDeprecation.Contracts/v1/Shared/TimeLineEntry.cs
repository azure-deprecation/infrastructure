using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class TimeLineEntry
    {
        [JsonProperty("phase")]
        public string? Phase { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("isDueDate")]
        public bool IsDueDate { get; set; }
    }
}
