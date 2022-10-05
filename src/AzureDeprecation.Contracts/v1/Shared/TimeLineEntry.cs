using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class TimeLineEntry
    {
        [JsonProperty("phase"), JsonPropertyName("phase")]
        public string? Phase { get; set; }
        
        [JsonProperty("date"), JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }
        
        [JsonProperty("description"), JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonProperty("isDueDate"), JsonPropertyName("isDueDate")]
        public bool IsDueDate { get; set; }
    }
}
