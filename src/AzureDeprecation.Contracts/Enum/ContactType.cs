using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContactType
    {
        Unknown,
        NotAvailable,
        Email,
        Support,
        MicrosoftQAndA
    }
}