namespace AzureDeprecation.APIs.REST.Contracts;

public class DeprecationInfo
{
    public string Id { get; set; } = null!;

    public string? Title { get; set; } = null!;

    public List<TimeLineEntry> Timeline { get; set; } = new();
    
    public Impact? Impact { get; set; }
    
    public Notice? Notice { get; set; }
    
    public string? RequiredAction { get; set; }
    
    public List<ContactEntry> Contact { get; set; } = new();

    public Dictionary<ExternalLinkType, string> Links { get; set; } = new();
}