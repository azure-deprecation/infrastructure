using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts.Enum
{
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
}