using System.Text.Json.Serialization;

namespace AzureDeprecation.Contracts.Enum
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ContactType
    {
        Unknown,
        NotAvailable,
        Email,
        Support,
        MicrosoftQAndA
    }
}