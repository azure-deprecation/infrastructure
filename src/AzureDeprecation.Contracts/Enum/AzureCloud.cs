using System.Text.Json.Serialization;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AzureCloud
    {
        Unknown,
        Public,
        Sovereign,
        China,
        Government,
        German
    }
}