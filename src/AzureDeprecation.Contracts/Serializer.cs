using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzureDeprecation.Contracts
{
    public static class Serializer
    {
        public static TContract? Deserialize<TContract>(string rawPayload)
        {
            var message = JsonConvert.DeserializeObject<TContract>(rawPayload);
            return message;
        }

        public static string Serialize<TContract>(TContract outputMessage)
        {
            var rawMessageBody = JsonConvert.SerializeObject(outputMessage, new StringEnumConverter());
            return rawMessageBody;
        }
    }
}