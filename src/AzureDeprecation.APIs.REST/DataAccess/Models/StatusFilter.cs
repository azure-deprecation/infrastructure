using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace AzureDeprecation.APIs.REST.DataAccess.Models
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter), true)]
    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StatusFilter
    {
        Opened,
        Closed
    }
}