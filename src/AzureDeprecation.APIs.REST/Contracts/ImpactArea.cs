using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.APIs.REST.Contracts;

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