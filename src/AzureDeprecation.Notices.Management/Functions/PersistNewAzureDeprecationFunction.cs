using AutoMapper;
using Azure.Messaging.ServiceBus;
using AzureDeprecation.Contracts.v1.Documents;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Notices.Management.Repositories.Interfaces;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureDeprecation.Notices.Management.Functions
{
    public partial class PersistNewAzureDeprecationFunction
    {
        private readonly ILogger<PersistNewAzureDeprecationFunction> _logger;
        private readonly IDeprecationsRepository _deprecationsRepository;
        private readonly IMapper _mapper;

        public PersistNewAzureDeprecationFunction(IDeprecationsRepository deprecationsRepository, IMapper mapper, ILogger<PersistNewAzureDeprecationFunction> logger)
        {
            _deprecationsRepository = deprecationsRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [FunctionName("persist-new-notice")]
        public async Task Run(
            [ServiceBusTrigger("new-deprecation-notices-tom", "persist-new-notice", Connection = "ServiceBus_ConnectionString")]
            ServiceBusReceivedMessage receivedSubscriptionMessage)
        {
            var stopwatch = ValueStopwatch.StartNew();
            using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                ["ServiceBusQueueMessageId"] = receivedSubscriptionMessage.MessageId,
                ["ServiceBusQueueMessageCorrelationId"] = receivedSubscriptionMessage.CorrelationId
            });

            try
            {
                var newDeprecationNoticePublishedV1Message = receivedSubscriptionMessage.Body.ToObjectFromJson<NewDeprecationNoticePublishedV1Message>();
                if (string.IsNullOrWhiteSpace(newDeprecationNoticePublishedV1Message?.Id))
                {
                    LogNoDeprecationIdWasFound();
                    return;
                }

                var doesDeprecationExist = await _deprecationsRepository.DoesDeprecationExistAsync(newDeprecationNoticePublishedV1Message.Id);
                if (doesDeprecationExist)
                {
                    LogDeprecationAlreadyExists(newDeprecationNoticePublishedV1Message.Id);
                    return;
                }

                // Map contracts
                var deprecationNoticeDocument = _mapper.Map<DeprecationNoticeDocument>(newDeprecationNoticePublishedV1Message);
                deprecationNoticeDocument.CreatedAt = newDeprecationNoticePublishedV1Message.PublishedNotice!.CreatedAt;
                deprecationNoticeDocument.LastUpdatedAt = deprecationNoticeDocument.CreatedAt;

                // Persist deprecation
                await _deprecationsRepository.PersistDeprecationAsync(deprecationNoticeDocument);
                LogDeprecationPersisted(newDeprecationNoticePublishedV1Message.Id);
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
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);

        [LoggerMessage(EventId = 201, EventName = "Timing", Level = LogLevel.Debug,
            Message = "No ID for the new deprecation notice was found. Skipping message.")]
        partial void LogNoDeprecationIdWasFound();

        [LoggerMessage(EventId = 202, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Deprecation with ID {DeprecationId} already exists.")]
        partial void LogDeprecationAlreadyExists(string deprecationId);

        [LoggerMessage(EventId = 203, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Deprecation with ID {DeprecationId} has been stored.")]
        partial void LogDeprecationPersisted(string deprecationId);

        [LoggerMessage(EventId = 500, EventName = "ErrorMessageProcessingFailed", Level = LogLevel.Error,
            Message = "Failed processing Service Bus queue message. Timing: {ElapsedMilliseconds} ms.")]
        partial void LogFailedProcessing(double elapsedMilliseconds);
    }
}