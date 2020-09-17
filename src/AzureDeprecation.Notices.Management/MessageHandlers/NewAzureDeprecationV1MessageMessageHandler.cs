using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Integrations.GitHub.Repositories;
using GuardNet;
using Octokit;

namespace AzureDeprecation.Notices.Management.MessageHandlers
{
    public class NewAzureDeprecationNotificationV1MessageHandler : ServiceBusMessageHandler<NewAzureDeprecationV1Message, NewDeprecationNoticePublishedV1Message>
    {
        private readonly GitHubRepository _gitHubRepository;

        public NewAzureDeprecationNotificationV1MessageHandler(GitHubRepository gitHubRepository)
        {
            Guard.NotNull(gitHubRepository,nameof(gitHubRepository));

            _gitHubRepository = gitHubRepository;
        }

        protected override async Task<NewDeprecationNoticePublishedV1Message> ProcessMessageAsync(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            // Determine deprecation year
            var deprecationYear = newNoticeV1MessageQueueMessage.DueOn.Year;
            
            // Get matching milestone
            var milestoneDueDate = DateTimeOffset.Parse($"12/31/{deprecationYear}").AddDays(1);
            var milestone = await _gitHubRepository.GetOrCreateMilestoneAsync(deprecationYear.ToString(), $"All Azure deprecation notices which are closing in {deprecationYear}", milestoneDueDate);

            // Generate issue content
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newNoticeV1MessageQueueMessage);

            // Determine all required labels
            var labels = DetermineRequiredLabels(newNoticeV1MessageQueueMessage);

            // Create GitHub issue
            var createdIssue = await _gitHubRepository.CreateIssueAsync(newNoticeV1MessageQueueMessage.Title, issueContent, milestone, labels);
            
            return GenerateNewDeprecationNoticePublishedV1Message(newNoticeV1MessageQueueMessage, createdIssue);
        }

        private List<string> DetermineRequiredLabels(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            var labels = new List<string>
            {
                "verified"
            };

            foreach (var service in newNoticeV1MessageQueueMessage.Impact.Services)
            {
                var label = LabelFactory.GetForService(service);
                labels.Add(label);
            }

            var areaLabel = LabelFactory.GetForImpactArea(newNoticeV1MessageQueueMessage.Impact.Area);
            labels.Add(areaLabel);
            var typeLabel = LabelFactory.GetForImpactType(newNoticeV1MessageQueueMessage.Impact.Type);
            labels.Add(typeLabel);

            return labels;
        }

        private NewDeprecationNoticePublishedV1Message GenerateNewDeprecationNoticePublishedV1Message(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, Issue createdIssue)
        {
            return new NewDeprecationNoticePublishedV1Message
            {
                DeprecationInfo = newNoticeV1MessageQueueMessage,
                ReportInfo = createdIssue
            };
        }
    }
}