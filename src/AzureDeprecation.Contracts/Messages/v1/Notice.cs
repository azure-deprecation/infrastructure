namespace AzureDeprecation.Contracts.Messages.v1
{
    public class Notice
    {
        public string? Description { get; set; }
        public List<string> Links { get; set; } = new();
    }
}