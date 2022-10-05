using System.Text.Json.Serialization;
using AzureDeprecation.Contracts.v1.Shared;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Messages
{
    public class NewDeprecationNoticePublishedV1Message
    {
        [JsonProperty("id"), JsonPropertyName("id")]
        public string? Id { get; set; }
        
        [JsonProperty("deprecationInfo"), JsonPropertyName("deprecationInfo")]
        public DeprecationInfo? DeprecationInfo { get; set; }
        
        [JsonProperty("publishedNotice"), JsonPropertyName("publishedNotice")]
        public PublishedNotice? PublishedNotice { get; set; }
    }
}