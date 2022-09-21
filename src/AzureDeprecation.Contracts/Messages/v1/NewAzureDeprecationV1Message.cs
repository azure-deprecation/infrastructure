namespace AzureDeprecation.Contracts.Messages.v1
{
    public class NewAzureDeprecationV1Message
    {
        public string? Title { get; set; }
        public List<InputTimeLineEntry> Timeline { get; set; } = new();
        public Impact? Impact { get; set; }
        public DraftNotice? Notice { get; set; }
        public RequiredAction? RequiredAction { get; set; }
        public List<ContactEntry> Contact { get; set; } = new();
        public string? AdditionalInformation { get; set; }

        public DateTimeOffset GetDueDate()
        {
            if (Timeline is null || Timeline.Count == 0)
            {
                throw new Exception("No timeline was provided.");
            }

            if (Timeline.Count == 1)
            {
                return Timeline[0].Date;
            }

            return Timeline.Where(x => x.IsDueDate).Max(x => x.Date);
        }
    }
}