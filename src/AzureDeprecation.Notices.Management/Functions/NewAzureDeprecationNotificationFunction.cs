using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts;
using AzureDeprecation.Notices.Management.MessageHandlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SBEntityType = Microsoft.Azure.WebJobs.ServiceBus.ServiceBusEntityType;

namespace AzureDeprecation.Notices.Management.Functions
{
    public class NewAzureDeprecationNotificationFunction
    {
        // TODO: Use custom GitHub Output binding
        //   Reference:
        //   -https://blog.maartenballiauw.be/post/2019/07/30/indexing-searching-nuget-with-azure-functions-and-search.html
        //   - https://github.com/ealsur/functions-extension-101)
        private readonly IServiceProvider _services;
        public NewAzureDeprecationNotificationFunction(IServiceProvider services)
        {
            _services = services;
        }

        [FunctionName("publish-new-notice")]
        public async Task Run([ServiceBusTrigger("new-azure-deprecation", Connection = "ServiceBus_ConnectionString")] ServiceBusReceivedMessage queueMessage,
                              [ServiceBus("new-deprecation-notices", SBEntityType.Topic, Connection = "ServiceBus_ConnectionString")] IAsyncCollector<ServiceBusMessage> publishedDeprecationNotice,
                              ILogger log)
        {
            if (!queueMessage.ApplicationProperties.TryGetValue("MessageType", out var rawMessageType) ||
                rawMessageType is not string messageTypeString ||
                !Enum.TryParse<MessageType>(messageTypeString, true, out var messageType))
            {
                throw new ArgumentException("No message type was annotated to the message.");
            }

            ServiceBusMessage outputMessage;
            switch (messageType)
            {
                case MessageType.NewAzureDeprecationV1:
                    var messageHandler = _services.GetRequiredService<NewAzureDeprecationNotificationV1MessageHandler>();
                    outputMessage = await messageHandler.ProcessAsync(queueMessage);
                    break;
                default:
                    throw new NotSupportedException($"Message type '{messageType}' is not supported");
            }

            await publishedDeprecationNotice.AddAsync(outputMessage);
        }
    }
}
