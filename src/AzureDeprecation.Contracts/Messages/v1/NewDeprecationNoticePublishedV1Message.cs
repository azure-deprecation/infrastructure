using Octokit;

namespace AzureDeprecation.Contracts.Messages.v1
{
    public class NewDeprecationNoticePublishedV1Message
    {
        public NewAzureDeprecationV1Message DeprecationInfo { get; set; }
        public Issue ReportInfo { get; set; }
    }
}
