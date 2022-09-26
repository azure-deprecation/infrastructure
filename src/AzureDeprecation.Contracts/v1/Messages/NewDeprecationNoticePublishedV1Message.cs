using AzureDeprecation.Contracts.v1.Shared;

namespace AzureDeprecation.Contracts.v1.Messages
{
    public class NewDeprecationNoticePublishedV1Message
    {
        public string? Id { get; set; }
        public DeprecationInfo? DeprecationInfo { get; set; }
        public PublishedNotice? PublishedNotice { get; set; }
    }
}