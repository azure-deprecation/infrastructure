using AutoMapper;
using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts;
using AzureDeprecation.Contracts.v1.Documents;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureDeprecation.Notices.Management.Functions
{
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
        public Task Run(
            [ServiceBusTrigger("new-deprecation-notices-tom", "persist-new-notice", Connection = "ServiceBus_ConnectionString")]
            ServiceBusReceivedMessage receivedSubscriptionMessage)
            //[CosmosDB(databaseName: "%CosmosDb_DatabaseName%", containerName: "%CosmosDb_ContainerName%", Connection = "CosmosDb_ConnectionString")]
            //IAsyncCollector<DeprecationNoticeDocument> documentsToPersist)
        {
            var stopwatch = ValueStopwatch.StartNew();
            using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                ["ServiceBusQueueMessageId"] = receivedSubscriptionMessage.MessageId,
                ["ServiceBusQueueMessageCorrelationId"] = receivedSubscriptionMessage.CorrelationId
            });

            try
            {
                // Map contracts
                // TODO: Implement test
                var newDeprecationNoticePublishedV1Message = receivedSubscriptionMessage.Body.ToObjectFromJson<NewDeprecationNoticePublishedV1Message>();
                var deprecationNoticeDocument = _mapper.Map<DeprecationNoticeDocument>(newDeprecationNoticePublishedV1Message);
                deprecationNoticeDocument.CreatedAt = newDeprecationNoticePublishedV1Message.PublishedNotice!.CreatedAt;
                deprecationNoticeDocument.LastUpdatedAt = deprecationNoticeDocument.CreatedAt;

                // Persist
                //await documentsToPersist.AddAsync(deprecationNoticeDocument).ConfigureAwait(false);
            }
            catch (Exception)
            {
                LogFailedProcessing(stopwatch.GetElapsedTotalMilliseconds());
                throw;
            }
            finally
            {
                LogTiming(stopwatch.GetElapsedTotalMilliseconds());
            }

            return Task.CompletedTask;
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);

        [LoggerMessage(EventId = 500, EventName = "ErrorMessageProcessingFailed", Level = LogLevel.Error,
            Message = "Failed processing Service Bus queue message. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogFailedProcessing(double elapsedMilliseconds);
    }
}