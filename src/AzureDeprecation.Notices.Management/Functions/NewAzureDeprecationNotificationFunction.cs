using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts;
using AzureDeprecation.Notices.Management.MessageHandlers;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SBEntityType = Microsoft.Azure.WebJobs.ServiceBus.ServiceBusEntityType;

namespace AzureDeprecation.Notices.Management.Functions
{
    public partial class NewAzureDeprecationNotificationFunction
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<NewAzureDeprecationNotificationFunction> _logger;

        public NewAzureDeprecationNotificationFunction(
            IServiceProvider services,
            ILogger<NewAzureDeprecationNotificationFunction> logger)
        {
            _services = services;
            _logger = logger;
        }

        [FunctionName("publish-new-notice")]
        public async Task Run(
            [ServiceBusTrigger("new-azure-deprecation", Connection = "ServiceBus_ConnectionString")]
            ServiceBusReceivedMessage queueMessage,
            [ServiceBus("new-deprecation-notices", SBEntityType.Topic, Connection = "ServiceBus_ConnectionString")]
            IAsyncCollector<ServiceBusMessage> publishedDeprecationNotice)
        {
            var stopwatch = ValueStopwatch.StartNew();
            using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                ["ServiceBusQueueMessageId"] = queueMessage.MessageId,
                ["ServiceBusQueueMessageCorrelationId"] = queueMessage.CorrelationId
            });
            if (!queueMessage.ApplicationProperties.TryGetValue("MessageType", out var rawMessageType) ||
                rawMessageType is not string messageTypeString)
            {
                LogRejectedQueueMessageWithNoMessageType(stopwatch.GetElapsedTotalMilliseconds());
                throw new ArgumentException(
                    $@"The annotated message type was either missing or invalid. Currently supported types are: [""{nameof(MessageType.NewAzureDeprecationV1)}""].");
            }

            if (!Enum.TryParse<MessageType>(messageTypeString, true, out var messageType))
            {
                LogRejectedQueueMessageWithInvalidMessageType(messageTypeString, stopwatch.GetElapsedTotalMilliseconds());
                throw new ArgumentException($@"Invalid annotated message type. Currently supported types are: [""{nameof(MessageType.NewAzureDeprecationV1)}""].");
            }

            try
            {
                ServiceBusMessage? outputMessage;
                switch (messageType)
                {
                    case MessageType.NewAzureDeprecationV1:
                        var messageHandler =
                            _services.GetRequiredService<NewAzureDeprecationNotificationV1MessageHandler>();
                        outputMessage = await messageHandler.ProcessAsync(queueMessage);
                        break;
                    default:
                        LogRejectedQueueMessageWithUnsupportedMessageType(messageType,
                            stopwatch.GetElapsedTotalMilliseconds());
                        throw new NotSupportedException($"Message type '{messageType}' is not supported");
                }

                if (outputMessage is null)
                {
                    throw new Exception("Message processing failed.");
                }

                await publishedDeprecationNotice.AddAsync(outputMessage).ConfigureAwait(false);
            }
            catch (Exception)
            {
                LogFailedProcessing(messageType, stopwatch.GetElapsedTotalMilliseconds());
                throw;
            }
            finally
            {
                LogTiming(stopwatch.GetElapsedTotalMilliseconds());
            }
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);

        [LoggerMessage(EventId = 400, EventName = "ErrorMissingMessageType", Level = LogLevel.Error,
            Message = "Missing message type property for Service Bus queue message. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogRejectedQueueMessageWithNoMessageType(double elapsedMilliseconds);

        [LoggerMessage(EventId = 401, EventName = "ErrorInvalidMessageType", Level = LogLevel.Error,
            Message = "Invalid message type `{MessageTypeString}` for Service Bus queue message. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogRejectedQueueMessageWithInvalidMessageType(string messageTypeString, double elapsedMilliseconds);

        [LoggerMessage(EventId = 402, EventName = "ErrorUnsupportedMessageType", Level = LogLevel.Error,
            Message = "Encountered unsupported message type: `{MessageType}`. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogRejectedQueueMessageWithUnsupportedMessageType(MessageType messageType, double elapsedMilliseconds);

        [LoggerMessage(EventId = 500, EventName = "ErrorMessageProcessingFailed", Level = LogLevel.Error,
            Message = "Failed processing Service Bus queue message with type: `{MessageType}`. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogFailedProcessing(MessageType messageType, double elapsedMilliseconds);
    }
}