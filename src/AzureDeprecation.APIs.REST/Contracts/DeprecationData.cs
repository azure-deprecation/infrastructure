namespace AzureDeprecation.APIs.REST.Contracts;

public class DeprecationInfo
{
    public string Id { get; set; } = null!;

    public DeprecationData DeprecationData { get; set; } = new();
    
    public PublishedNotice PublishedNotice { get; set; } = new();
}

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

public class ApiInfo
{
    public string? Url { get; set; }
    public int Id { get; set; }
}

public class DashboardInfo
{
    public string? Url { get; set; }
    public int Id { get; set; }
}

public class DeprecationData
{
    public string? Title { get; set; }
    public List<TimeLineEntry> Timeline { get; set; } = new();
    public Impact? Impact { get; set; }
    public Notice? Notice { get; set; }
    public string? RequiredAction { get; set; }
    public List<ContactEntry> Contact { get; set; } = new();
    public string? AdditionalInformation { get; set; }
}