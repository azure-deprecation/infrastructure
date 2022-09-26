using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.Contracts.v1.Documents;

namespace AzureDeprecation.APIs.REST.DataAccess.Interfaces;

public interface IDeprecationsRepository
{
    /// <summary>
    /// Get List of deprecations by filter
    /// </summary>
    IAsyncEnumerable<DeprecationNoticeDocument> GetDeprecationsAsync(DeprecationsRequestModel deprecationsRequestModel,
        CancellationToken cancellation = default);
}