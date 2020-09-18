using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Arcus.EventGrid.Publishing;
using AzureDeprecation.Contracts;
using AzureDeprecation.Contracts.Messages.v1;
using CloudNative.CloudEvents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureDeprecation.Notifications.Functions
{
    public class EventGridNotificationsFunction
    {
        private readonly IConfiguration _configuration;

        public EventGridNotificationsFunction(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [FunctionName("event-grid-notification")]
        public async Task Run([ServiceBusTrigger("new-deprecation-notices", "event-grid-notifications", Connection = "ServiceBus_ConnectionString")] NewDeprecationNoticePublishedV1Message newDeprecationNoticePublishedV1Message, ILogger log)
        {
            var eventGridTopicEndpoint = _configuration["EVENTGRID_ENDPOINT"];
            var eventGridAuthKey = _configuration["EVENTGRID_AUTH_KEY"];

            var eventGridPublisher = EventGridPublisherBuilder
                .ForTopic(eventGridTopicEndpoint)
                .UsingAuthenticationKey(eventGridAuthKey)
                .Build();
            
            var @event = new CloudEvent(CloudEventsSpecVersion.V1_0,
                "NewDeprecationNoticePublishedV1",
                new Uri("https://eventgrid.arcus-azure.net/"),
                subject: $"/{newDeprecationNoticePublishedV1Message.DeprecationInfo.Impact.Services.First()}")
            {
                Data = Serializer.Serialize(newDeprecationNoticePublishedV1Message),
                DataContentType = new ContentType("application/json")
            };

            await eventGridPublisher.PublishAsync(@event);
        }
    }
}
