using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace AzureDeprecation.Notices.Management.MessageHandlers
{
    public abstract class ServiceBusMessageHandler<TInputMessage, TOutputMessage>
        where TInputMessage : class
    {
        public async Task<Message> ProcessAsync(Message queueMessage)
        {
            var message = DeserializeMessageBody(queueMessage);
            var outputPayload = await ProcessMessageAsync(message);
            var serializedMessage = SerializeMessageBody(outputPayload);
            return new Message(serializedMessage);
        }

        private static TInputMessage DeserializeMessageBody(Message queueMessage)
        {
            var rawMessageBody = Encoding.UTF8.GetString(queueMessage.Body);
            var message = JsonConvert.DeserializeObject<TInputMessage>(rawMessageBody);
            return message;
        }

        private static byte[] SerializeMessageBody(TOutputMessage outputMessage)
        {
            var rawMessageBody = JsonConvert.SerializeObject(outputMessage);
            var messageBytes = Encoding.UTF8.GetBytes(rawMessageBody);
            return messageBytes;
        }

        protected abstract Task<TOutputMessage> ProcessMessageAsync(TInputMessage message);
    }
}