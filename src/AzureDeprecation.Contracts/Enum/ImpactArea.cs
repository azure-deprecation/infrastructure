using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
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
