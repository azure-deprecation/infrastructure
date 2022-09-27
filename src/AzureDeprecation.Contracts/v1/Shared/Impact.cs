using AzureDeprecation.Contracts.Enum;
using Newtonsoft.Json;

namespace AzureDeprecation.Contracts.v1.Shared
{
    public class Impact
    {
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("type")]
        public ImpactType Type { get; set; }

        [JsonProperty("area")]
        public ImpactArea Area { get; set; }

        [JsonProperty("cloud")]
        public AzureCloud Cloud { get; set; }

        [JsonProperty("services")]
        public List<AzureService> Services { get; set; } = new();
    }
}