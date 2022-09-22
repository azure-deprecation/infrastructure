using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.APIs.REST.Contracts;

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