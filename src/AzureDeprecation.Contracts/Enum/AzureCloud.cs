using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
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