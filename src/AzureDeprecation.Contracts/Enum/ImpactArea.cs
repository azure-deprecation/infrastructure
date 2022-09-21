using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AzureDeprecation.Contracts.Enum
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter), true)]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
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
