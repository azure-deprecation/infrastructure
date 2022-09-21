using AzureDeprecation.Contracts.Interfaces;
using AzureDeprecation.Contracts.v1.Shared;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Documents
{
    public class DeprecationNoticeDocument : ICosmosDbDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("schemaVersion")]
        public string SchemaVersion => "v1";

        [JsonProperty("deprecationInfo")]
        public DeprecationInfo? DeprecationInfo { get; set; }

        [JsonProperty("publishedNotice")]
        public PublishedNotice? PublishedNotice { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("lastUpdatedAt")]
        public DateTimeOffset? LastUpdatedAt { get; set; }
    }
}
