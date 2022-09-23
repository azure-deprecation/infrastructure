using AzureDeprecation.Contracts.Messages.v1;

namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class NoticeEntity
{
    public string Id { get; set; } = null!;

    public DeprecationInfo DeprecationInfo { get; set; } = new();
    
    public PublishedNotice PublishedNotice { get; set; } = new();
}