using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Contracts.Messages.v1;
using Humanizer;

namespace AzureDeprecation.Notices.Management
{
    public class IssueFactory
    {        
        public static string GenerateNewDeprecationNotice(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage)
        {
            var issueBuilder = new StringBuilder();

            WriteIntro(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteNotice(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteTimeline(newNoticeV1MessageQueueMessage.Timeline, issueBuilder);
            WriteImpact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteRequiredAction(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteContact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteMoreInformation(newNoticeV1MessageQueueMessage, issueBuilder);

            return issueBuilder.ToString();
        }

        private static void WriteIntro(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            var dueDate = newNoticeV1MessageQueueMessage.GetDueDate();
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Title);
            issueBuilder.AppendLine();
            issueBuilder.AppendLine($"**Deadline:** {dueDate:MMM dd, yyyy}");
            issueBuilder.AppendLine("**Impacted Services:**");

            foreach (var impactedService in newNoticeV1MessageQueueMessage.Impact.Services)
            {
                issueBuilder.AppendLine($"- Azure {impactedService.Humanize(LetterCasing.Title)}");
            }

            issueBuilder.AppendLine();

            if (newNoticeV1MessageQueueMessage.Notice.Links?.Any() == true)
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
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Notice.Description);
        }

        private static void WriteImpact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Impact");
            issueBuilder.AppendLine();
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Impact.Description);
            issueBuilder.AppendLine();
        }

        private static void WriteTimeline(List<InputTimeLineEntry> timeline, StringBuilder issueBuilder)
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

        private static void WriteTimelineForSinglePhase(StringBuilder issueBuilder, InputTimeLineEntry timelineEntry)
        {
            var phase = string.IsNullOrWhiteSpace(timelineEntry.Phase) ? "Deprecation" : timelineEntry.Phase;
            var description = string.IsNullOrWhiteSpace(timelineEntry.Description) ? "N/A" : timelineEntry.Description;
            issueBuilder.AppendLine($"|{phase}|{timelineEntry.Date:MMM dd, yyyy}|{description}|");
        }

        private static void WriteTimelineEntriesForAllPhases(List<InputTimeLineEntry> timeline, StringBuilder issueBuilder)
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
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.RequiredAction.Description);
        }

        private static void WriteContact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Contact");
            issueBuilder.AppendLine();

            if (newNoticeV1MessageQueueMessage.Contact.Count == 1)
            {
                var contactInformation = GetContactInformation(newNoticeV1MessageQueueMessage.Contact.First());
                issueBuilder.AppendLine($"{contactInformation}");
            }
            else
            {
                issueBuilder.AppendLine("You can get in touch through the following options:");
                foreach (var contactEntry in newNoticeV1MessageQueueMessage.Contact)
                {
                    var contactInformation = GetContactInformation(contactEntry);
                    issueBuilder.AppendLine($"- {contactInformation}");
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
                    return $"Get answers from Microsoft Q&A ([link](mailto:{contactEntry.Data})).";
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