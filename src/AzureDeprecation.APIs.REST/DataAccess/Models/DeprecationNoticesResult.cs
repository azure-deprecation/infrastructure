using AzureDeprecation.Contracts.v1.Documents;

namespace AzureDeprecation.APIs.REST.DataAccess.Models;

public class DeprecationNoticesResult
{
    public List<DeprecationNoticeDocument> Deprecations { get; set; } = new();
}