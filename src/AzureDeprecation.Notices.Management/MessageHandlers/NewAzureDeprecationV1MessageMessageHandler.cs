using AutoMapper;
using AzureDeprecation.Contracts.v1.Messages;
using AzureDeprecation.Contracts.v1.Shared;
using AzureDeprecation.Integrations.GitHub.Repositories;
using GuardNet;
using Microsoft.Extensions.Logging;
using Octokit;

namespace AzureDeprecation.Notices.Management.MessageHandlers
{
    public class NewAzureDeprecationNotificationV1MessageHandler : ServiceBusMessageHandler<NewAzureDeprecationV1Message, NewDeprecationNoticePublishedV1Message>
    {
        private readonly ILogger<NewAzureDeprecationNotificationV1MessageHandler> _logger;
        private readonly GitHubRepository _gitHubRepository;
        private readonly IMapper _mapper;

        public NewAzureDeprecationNotificationV1MessageHandler(GitHubRepository gitHubRepository, IMapper mapper, ILogger<NewAzureDeprecationNotificationV1MessageHandler> logger)
        {
            Guard.NotNull(gitHubRepository, nameof(gitHubRepository));
            Guard.NotNull(mapper, nameof(mapper));
            Guard.NotNull(logger, nameof(logger));

            _mapper = mapper;
            _logger = logger;
            _gitHubRepository = gitHubRepository;
        }

        protected override async Task<NewDeprecationNoticePublishedV1Message> ProcessMessageAsync(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            // Create notice issue
            var createdIssue = await CreateDeprecationNoticeAsync(newNoticeV1MessageQueueMessage);

            // Post discussion message
            await PostCommentToSteerPeopleToGitHubDiscussionsAsync(createdIssue);

            // Lock conversation
            await LockConversationAsync(createdIssue);

            var uniqueNoticeId = Guid.NewGuid().ToString();
            var messageToPublish = GenerateNewDeprecationNoticePublishedV1Message(uniqueNoticeId, newNoticeV1MessageQueueMessage, createdIssue);

            // Provide logging
            this._logger.LogInformation("Created GitHub issue #{IssueId} which is available on {IssueUrl} for deprecation with ID {DeprecationId}", createdIssue.Id, createdIssue.Url, uniqueNoticeId);

            return messageToPublish;
        }

        private async Task PostCommentToSteerPeopleToGitHubDiscussionsAsync(Issue createdIssue)
        {
            var commentText = "This issue is automatically managed and suggest to use [GitHub Discussions](https://github.com/azure-deprecation/dashboard/discussions) to discuss this deprecation.";
            await _gitHubRepository.PostCommentAsync(createdIssue.Number, commentText);
        }

        private async Task LockConversationAsync(Issue createdIssue)
        {
            await _gitHubRepository.LockIssueAsync(createdIssue.Number);
        }

        private async Task<Issue> CreateDeprecationNoticeAsync(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            // Determine deprecation year
            var deprecationYear = newNoticeV1MessageQueueMessage.GetDueDate().Year;

            // Get matching milestone
            var milestoneDueDate = new DateTimeOffset(new DateTime(deprecationYear + 1, 1, 1));
            var milestone = await _gitHubRepository.GetOrCreateMilestoneAsync(deprecationYear.ToString(),
                $"All Azure deprecation notices which are closing in {deprecationYear}", milestoneDueDate);
            
            // Get matching project
            var project = await _gitHubRepository.GetOrCreateProjectAsync("Deprecation Notice Timeline");

            // Generate issue content
            var issueContent = IssueFactory.GenerateNewDeprecationNotice(newNoticeV1MessageQueueMessage);

            // Determine all required labels
            var labels = DetermineRequiredLabels(newNoticeV1MessageQueueMessage);

            // Create GitHub issue
            var createdIssue = await _gitHubRepository.CreateIssueAsync(newNoticeV1MessageQueueMessage.Title!, issueContent, milestone, labels);

            // Add GitHub issue to deprecation notice project
            await _gitHubRepository.AddIssueToProjectAsync(project.Id, deprecationYear.ToString(), createdIssue.Id);

            return createdIssue;
        }

        private List<string> DetermineRequiredLabels(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            var labels = new List<string>
            {
                "verified"
            };

            if (newNoticeV1MessageQueueMessage.Impact is not null)
            {
                foreach (var service in newNoticeV1MessageQueueMessage.Impact.Services)
                {
                    var label = LabelFactory.GetForService(service);
                    labels.Add(label);
                }

                var areaLabel = LabelFactory.GetForImpactArea(newNoticeV1MessageQueueMessage.Impact.Area);
                labels.Add(areaLabel);
                var typeLabel = LabelFactory.GetForImpactType(newNoticeV1MessageQueueMessage.Impact.Type);
                labels.Add(typeLabel);
                var cloudLabel = LabelFactory.GetForCloud(newNoticeV1MessageQueueMessage.Impact.Cloud);
                labels.Add(cloudLabel);
            }
            else
            {
                labels.Add("services:unknown");
                labels.Add("area:unknown");
                labels.Add("cloud:unknown");
                labels.Add("impact:unknown");
            }

            return labels;
        }

        private NewDeprecationNoticePublishedV1Message GenerateNewDeprecationNoticePublishedV1Message(string uniqueNoticeId, NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, Issue createdIssue)
        {
            var deprecationInfo = _mapper.Map<DeprecationInfo>(newNoticeV1MessageQueueMessage);
            var publishedNotice = _mapper.Map<PublishedNotice>(createdIssue);

            return new NewDeprecationNoticePublishedV1Message
            {
                Id = uniqueNoticeId,
                DeprecationInfo = deprecationInfo,
                PublishedNotice = publishedNotice
            };
        }
    }
}