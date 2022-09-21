using System.Text.Json.Serialization;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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