using System.Text;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class ComposedInfo
    {
        public string OfficialReport { get; set; }
        public string AdditionalInfo { get; set; }
        public string Description => ComposeMessage();

        private string ComposeMessage()
        {
            var messageBuilder = new StringBuilder();

            if (string.IsNullOrWhiteSpace(AdditionalInfo) == false)
            {
                messageBuilder.AppendLine(AdditionalInfo);
                messageBuilder.AppendLine();
            }

            if (string.IsNullOrWhiteSpace(OfficialReport) == false)
            {
                messageBuilder.AppendLine("Here's the official report from Microsoft:");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine(OfficialReport);
            }

            return messageBuilder.ToString();
        }
    }
}
