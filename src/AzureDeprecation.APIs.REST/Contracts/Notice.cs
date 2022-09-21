namespace AzureDeprecation.APIs.REST.Contracts;

public class Notice
{
    public string? Description { get; set; }
    public List<string> Links { get; set; } = new List<string>();
}