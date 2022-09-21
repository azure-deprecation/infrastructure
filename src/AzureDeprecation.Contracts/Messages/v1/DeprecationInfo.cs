namespace AzureDeprecation.Contracts.Messages.v1
{
    public class DeprecationInfo
    {
        public string? Title { get; set; }
        public List<TimeLineEntry> Timeline { get; set; } = new();
        public Impact? Impact { get; set; }
        public Notice? Notice { get; set; }
        public string? RequiredAction { get; set; }
        public List<ContactEntry> Contact { get; set; } = new();
        public string? AdditionalInformation { get; set; }
    }
}