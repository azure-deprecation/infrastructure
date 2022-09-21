using Newtonsoft.Json;

namespace AzureDeprecation.APIs.REST.Utils;

public static class JsonSettingsProvider
{
    public static JsonSerializerSettings GetDefault()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        return settings;
    }
}