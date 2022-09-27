using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts.Enum
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter), true)]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContactType
    {
        Unknown,
        NotAvailable,
        Email,
        Support,
        MicrosoftQAndA
    }
}