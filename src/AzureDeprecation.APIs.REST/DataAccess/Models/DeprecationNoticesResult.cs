namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class DeprecationNoticesResult
{
    public List<NoticeEntity> Deprecations { get; set; } = new();
}