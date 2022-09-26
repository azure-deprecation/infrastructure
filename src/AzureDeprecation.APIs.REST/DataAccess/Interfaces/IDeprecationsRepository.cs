using AzureDeprecation.APIs.REST.DataAccess.Models;

namespace AzureDeprecation.APIs.REST.DataAccess.Interfaces;

public interface IDeprecationsRepository
{
    /// <summary>
    /// Get List of deprecations by filter
    /// </summary>
    IAsyncEnumerable<NoticeEntity> GetDeprecationsAsync(DeprecationsRequestModel deprecationsRequestModel,
        CancellationToken cancellation = default);

    /// <summary>
    /// Get deprecations by its id
    /// </summary>
    Task<NoticeEntity> GetDeprecationAsync(Guid id, CancellationToken cancellation = default);
}