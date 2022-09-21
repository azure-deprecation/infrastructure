namespace AzureDeprecation.Contracts.Messages.v1
{
    public class PublishedNotice
    {
        public string? Title { get; set; }
        public ApiInfo? ApiInfo { get; set; }
        public DashboardInfo? DashboardInfo { get; set; }
        public List<string> Labels { get; set; } = new();
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
    }
}