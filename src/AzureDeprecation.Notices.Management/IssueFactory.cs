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
            WriteImpact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteRequiredAction(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteContact(newNoticeV1MessageQueueMessage, issueBuilder);
            WriteMoreInformation(newNoticeV1MessageQueueMessage, issueBuilder);

            return issueBuilder.ToString();
        }

        private static void WriteIntro(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Title);
            issueBuilder.AppendLine();
            issueBuilder.AppendLine($"**Deadline:** {newNoticeV1MessageQueueMessage.DueOn:D}");
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
            if (string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.Notice.AdditionalInfo) == false)
            {
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Notice.AdditionalInfo);
                issueBuilder.AppendLine();
            }

            if (string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.Notice.OfficialReport) == false)
            {
                issueBuilder.AppendLine("Here's the official report from Microsoft:");
                issueBuilder.AppendLine();
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Notice.OfficialReport);
                issueBuilder.AppendLine();
            }
        }

        private static void WriteImpact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Impact");
            issueBuilder.AppendLine();
            issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.Impact.Description);
            issueBuilder.AppendLine();
        }

        private static void WriteRequiredAction(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Required Action");
            issueBuilder.AppendLine();
            if (string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.RequiredAction.AdditionalInfo) == false)
            {
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.RequiredAction.AdditionalInfo);
                issueBuilder.AppendLine();
            }

            if (string.IsNullOrWhiteSpace(newNoticeV1MessageQueueMessage.RequiredAction.OfficialReport) == false)
            {
                issueBuilder.AppendLine("Here's the official report from Microsoft:");
                issueBuilder.AppendLine();
                issueBuilder.AppendLine(newNoticeV1MessageQueueMessage.RequiredAction.OfficialReport);
                issueBuilder.AppendLine();
            }
        }

        private static void WriteContact(NewAzureDeprecationV1Message newNoticeV1MessageQueueMessage, StringBuilder issueBuilder)
        {
            issueBuilder.AppendLine("### Contact");
            issueBuilder.AppendLine();
            switch (newNoticeV1MessageQueueMessage.Contact.Type)
            {
                case ContactType.Support:
                    issueBuilder.AppendLine("Contact Azure support ([link](https://portal.azure.com/#blade/Microsoft_Azure_Support/HelpAndSupportBlade/overview)).");
                    break;
                case ContactType.NotAvailable:
                    issueBuilder.AppendLine("None.");
                    break;
            }

            issueBuilder.AppendLine();
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