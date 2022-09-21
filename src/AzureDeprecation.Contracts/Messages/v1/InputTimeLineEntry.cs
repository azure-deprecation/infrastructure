namespace AzureDeprecation.Contracts.Messages.v1
{
    public class InputTimeLineEntry
    {
        public string? Phase { get; set; }
        public DateTimeOffset Date { get; set; }
        public string? Description { get; set; }
        public bool IsDueDate { get; set; }
    }
    public class TimeLineEntry
    {
        public string? Phase { get; set; }
        public DateTimeOffset Date { get; set; }
        public string? Description { get; set; }
    }
}