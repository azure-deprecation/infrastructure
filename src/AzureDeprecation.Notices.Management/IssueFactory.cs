﻿using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Contracts.v1.Messages;
using Humanizer;
using System.Text;
using AzureDeprecation.Contracts.v1.Shared;

namespace AzureDeprecation.Notices.Management
{
    public class IssueFactory
    {        
        public static string GenerateNewDeprecationNotice(string uniqueDeprecationId, NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            var issueBuilder = new StringBuilder();

            WriteIntro(uniqueDeprecationId, newNoticeV1MessageQueueMessage, issueBuilder);
            WriteNotice(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteTimeline(newNoticeV1MessageQueueMessage.Timeline, issueBuilder);
            WriteImpact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteRequiredAction(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteContact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteMoreInformation(newNoticeV1MessageQueueMessage, issueBuilder);

            return issueBuilder.ToString();
        }

        private static void WriteIntro(string uniqueDeprecationId, NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            var dueDate = newNoticeV1MessageQueueMessage.GetDueDate();
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Title);
            issueBuilder.AppendLine();
            issueBuilder.AppendLine($"**Deprecation ID:** {uniqueDeprecationId}");
            issueBuilder.AppendLine($"**Deadline:** {dueDate:MMM dd, yyyy}");
            issueBuilder.AppendLine("**Impacted Services:**");

            if (newNoticeV1MessageQueueMessage.Impact is not null)
            {
                foreach (var impactedService in newNoticeV1MessageQueueMessage.Impact.Services)
                {
                    issueBuilder.AppendLine($"- Azure {impactedService.Humanize(LetterCasing.Title)}");
                }
            }

            issueBuilder.AppendLine();

            if (newNoticeV1MessageQueueMessage.Notice?.Links?.Any() == true)
            {
                issueBuilder.AppendLine("**More information:**");

                foreach (var link in newNoticeV1MessageQueueMessage.Notice.Links)
                {
                    issueBuilder.AppendLine($"- {link}");
                }
            }

            issueBuilder.AppendLine();
        }

        private static void WriteNotice(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Notice");
            issueBuilder.AppendLine();
            if (!string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.Notice?.Description))
            {
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Notice.Description);
            }
            else
            {
                issueBuilder.AppendLine("No notice information is available.");

            }
        }

        private static void WriteImpact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Impact");
            issueBuilder.AppendLine();
            if (!string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.Impact?.Description))
            {
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Impact.Description);
            }
            else
            {
                issueBuilder.AppendLine("No impact information is available.");

            }
            issueBuilder.AppendLine();
        }

        private static void WriteTimeline(List<TimeLineEntry> timeline, StringBuilder issueBuilder)
        {
            if (timeline == null || timeline.Any() == false)
            {
                return;
            }

            issueBuilder.AppendLine("### Timeline");
            issueBuilder.AppendLine();
            issueBuilder.AppendLine("| Phase | Date | Description |");
            issueBuilder.AppendLine("|:------|------|-------------|");

            if (timeline.Count > 1)
            {
                WriteTimelineEntriesForAllPhases(timeline, issueBuilder);
            }
            else
            {
                var timelineEntry = timeline.Single();
                WriteTimelineForSinglePhase(issueBuilder, timelineEntry);
            }

            issueBuilder.AppendLine();
        }

        private static void WriteTimelineForSinglePhase(StringBuilder issueBuilder, TimeLineEntry timelineEntry)
        {
            var phase = string.IsNullOrWhiteSpace(timelineEntry.Phase) ? "Deprecation" : timelineEntry.Phase;
            var description = string.IsNullOrWhiteSpace(timelineEntry.Description) ? "N/A" : timelineEntry.Description;
            issueBuilder.AppendLine($"|{phase}|{timelineEntry.Date:MMM dd, yyyy}|{description}|");
        }

        private static void WriteTimelineEntriesForAllPhases(List<TimeLineEntry> timeline, StringBuilder issueBuilder)
        {
            foreach (var entry in timeline.OrderBy(x => x.Date))
            {
                var phase = string.IsNullOrWhiteSpace(entry.Phase) && entry.IsDueDate ? "Deprecation" : entry.Phase;
                issueBuilder.AppendLine($"|{phase}|{entry.Date:MMM dd, yyyy}|{entry.Description}|");
            }
        }

        private static void WriteRequiredAction(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Required Action");
            issueBuilder.AppendLine();
            if (!string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.RequiredAction?.Description))
            {
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.RequiredAction.Description);
            }
            else
            {
                issueBuilder.AppendLine("No required action information is available.");

            }
        }

        private static void WriteContact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Contact");
            issueBuilder.AppendLine();

            switch (newNoticeV1MessageQueueMessage.Contact.Count)
            {
                case 0:
                    issueBuilder.AppendLine("No contact information is available.");
                    break;
                case 1:
                {
                    var contactInformation = GetContactInformation(newNoticeV1MessageQueueMessage.Contact.First());
                    issueBuilder.AppendLine($"{contactInformation}");
                    break;
                }
                default:
                {
                    issueBuilder.AppendLine("You can get in touch through the following options:");
                    foreach (var contactEntry in newNoticeV1MessageQueueMessage.Contact)
                    {
                        var contactInformation = GetContactInformation(contactEntry);
                        issueBuilder.AppendLine($"- {contactInformation}");
                    }

                    break;
                }
            }
            
            issueBuilder.AppendLine();
        }

        private static string GetContactInformation(ContactEntry contactEntry)
        {
            switch (contactEntry.Type)
            {
                case ContactType.Support:
                    return "Contact Azure support ([link](https://portal.azure.com/#blade/Microsoft_Azure_Support/HelpAndSupportBlade/overview)).";
                case ContactType.Email:
                    return $"Contact the product group through email ([email](mailto:{contactEntry.Data})).";
                case ContactType.MicrosoftQAndA:
                    return $"Get answers from Microsoft Q&A ([link]({contactEntry.Data})).";
                case ContactType.NotAvailable:
                    return "None.";
                case ContactType.Unknown:
                    return "No information was provided.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(contactEntry.Type), contactEntry.Type, "Contact type is not implemented.");
            }
        }

        private static void WriteMoreInformation(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            if(string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.AdditionalInformation) == false)
            {
                issueBuilder.AppendLine("### More information");
                issueBuilder.AppendLine();
                issueBuilder.Append(newNoticeV1MessageQueueMessage.AdditionalInformation);
            }
        }
    }
}