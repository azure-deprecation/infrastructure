using System;

namespace AzureDeprecation.APIs.REST.Contracts;

public class TimeLineEntry
{
    public string? Phase { get; set; }
    public DateTimeOffset Date { get; set; }
    public string? Description { get; set; }
}