using Arcus.EventGrid.Publishing;
using AzureDeprecation.Contracts;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Runtimes.AzureFunctions;
using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace AzureDeprecation.Notifications.Functions
{
    public partial class EventGridNotificationsFunction
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EventGridNotificationsFunction> _logger;

        public EventGridNotificationsFunction(
            IConfiguration configuration,
            ILogger<EventGridNotificationsFunction> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("event-grid-notification")]
        public async Task Run(
            [ServiceBusTrigger("new-deprecation-notices", "event-grid-notifications", Connection = "ServiceBus_ConnectionString")]
            NewDeprecationNoticePublishedV1Message newDeprecationNoticePublishedV1Message)
        {
            var stopwatch = ValueStopwatch.StartNew();

            var eventGridTopicEndpoint = _configuration["EVENTGRID_ENDPOINT"];
            var eventGridAuthKey = _configuration["EVENTGRID_AUTH_KEY"];

            var eventGridPublisher = EventGridPublisherBuilder
                .ForTopic(eventGridTopicEndpoint)
                .UsingAuthenticationKey(eventGridAuthKey)
                .Build();
            
            var @event = new CloudEvent(
                CloudEventsSpecVersion.V1_0,
                "NewDeprecationNoticePublishedV1",
                new Uri("https://github.com/azure-deprecation/dashboard"),
                subject: $"/{newDeprecationNoticePublishedV1Message.DeprecationInfo!.Impact!.Services.First()}")
            {
                Data = Serializer.Serialize(newDeprecationNoticePublishedV1Message),
                DataContentType = new ContentType("application/json")
            };

            await eventGridPublisher.PublishAsync(@event).ConfigureAwait(false);

            LogTiming(stopwatch.GetElapsedTotalMilliseconds());
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);
    }
}