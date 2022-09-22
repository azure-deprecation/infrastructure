using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.APIs.REST.Contracts;

[JsonConverter(typeof(StringEnumConverter))]
public enum ContactType
{
    Unknown,
    NotAvailable,
    Email,
    Support,
    MicrosoftQAndA
}