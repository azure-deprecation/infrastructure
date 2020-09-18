namespace AzureDeprecation.Contracts.Messages.v1
{
    public class NewDeprecationNoticePublishedV1Message
    {
        public NewAzureDeprecationV1Message DeprecationInfo { get; set; }
        public PublishedNotice PublishedNotice { get; set; }
    }
}
