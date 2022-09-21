using AutoMapper;
using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureDeprecation.Notices.Management.Functions
{
    public class DeprecationNoticeDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DeprecationInfo? DeprecationInfo { get; set; }
        public PublishedNotice? PublishedNotice { get; set; }
    }

    public partial class PersistNewAzureDeprecationFunction
    {
        private readonly ILogger<PersistNewAzureDeprecationFunction> _logger;
        private readonly IMapper _mapper;

        public PersistNewAzureDeprecationFunction(IMapper mapper, ILogger<PersistNewAzureDeprecationFunction> logger)
        {
            _logger = logger;
            _mapper = mapper;
        }

        [FunctionName("persist-new-notice")]
        public async Task Run(
            [ServiceBusTrigger("new-deprecation-notices-tom", "persist-new-notice", Connection = "ServiceBus_ConnectionString")]
            ServiceBusReceivedMessage receivedSubscriptionMessage,
            [CosmosDB(databaseName: "%CosmosDb_DatabaseName%", containerName: "%CosmosDb_ContainerName%", Connection = "CosmosDb_ConnectionString")]
            IAsyncCollector<DeprecationNoticeDocument> documentsToPersist)
        {
            var stopwatch = ValueStopwatch.StartNew();
            using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                ["ServiceBusQueueMessageId"] = receivedSubscriptionMessage.MessageId,
                ["ServiceBusQueueMessageCorrelationId"] = receivedSubscriptionMessage.CorrelationId
            });

            // Map contracts
            var newDeprecationNoticePublishedV1Message = receivedSubscriptionMessage.Body.ToObjectFromJson<NewDeprecationNoticePublishedV1Message>();
            var deprecationNoticeDocument = _mapper.Map<DeprecationNoticeDocument>(newDeprecationNoticePublishedV1Message);

            // Persist
            await documentsToPersist.AddAsync(deprecationNoticeDocument).ConfigureAwait(false);

            LogTiming(stopwatch.GetElapsedTotalMilliseconds());
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);

        [LoggerMessage(EventId = 500, EventName = "ErrorMessageProcessingFailed", Level = LogLevel.Error,
            Message = "Failed processing Service Bus queue message with type: `{MessageType}`. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogFailedProcessing(MessageType messageType, double elapsedMilliseconds);
    }
}