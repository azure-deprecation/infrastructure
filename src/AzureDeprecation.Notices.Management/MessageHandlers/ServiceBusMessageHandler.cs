using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts;
using System.Text;
using System.Threading.Tasks;

namespace AzureDeprecation.Notices.Management.MessageHandlers
{
    public abstract class ServiceBusMessageHandler<TInputMessage, TOutputMessage>
        where TInputMessage : class
    {
        public async Task<ServiceBusMessage> ProcessAsync(ServiceBusReceivedMessage queueMessage)
        {
            var message = DeserializeMessageBody(queueMessage);
            var outputPayload = await ProcessMessageAsync(message);
            var serializedMessage = SerializeMessageBody(outputPayload);
            return new ServiceBusMessage(serializedMessage);
        }

        private static TInputMessage DeserializeMessageBody(ServiceBusReceivedMessage queueMessage)
        {
            var rawMessageBody = Encoding.UTF8.GetString(queueMessage.Body);
            return Serializer.Deserialize<TInputMessage>(rawMessageBody);
        }

        private static byte[] SerializeMessageBody(TOutputMessage outputMessage)
        {
            var rawMessageBody=Serializer.Serialize(outputMessage);
            var messageBytes = Encoding.UTF8.GetBytes(rawMessageBody);
            return messageBytes;
        }

        protected abstract Task<TOutputMessage> ProcessMessageAsync(TInputMessage message);
    }
}