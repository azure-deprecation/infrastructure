using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.APIs.REST.Contracts;

[JsonConverter(typeof(StringEnumConverter))]
public enum ImpactType
{
    Unknown,
    None,
    Limited,
    UpgradeRequired,
    MigrationRequired,
    ShutdownWithoutAlternative
}