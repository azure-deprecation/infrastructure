using System.Text.Json.Serialization;
using AzureDeprecation.Contracts.Enum;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class ContactEntry
    {
        [JsonProperty("type"), JsonPropertyName("type")]
        public ContactType Type { get; set; }

        [JsonProperty("data"), JsonPropertyName("data")]
        public string? Data { get; set; }
    }
}