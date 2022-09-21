using System.Text.Json.Serialization;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImpactArea
    {
        Unknown,
        Sdk,
        Certification,
        Tooling,
        Security,
        ApiEndpoint,
        Feature,
        ServiceRuntime,
        Region,
        Sku
    }
}
