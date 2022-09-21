using System.Threading;
using System.Threading.Tasks;
using AzureDeprecation.APIs.REST.DataAccess.Models;

namespace AzureDeprecation.APIs.REST.DataAccess.Contracts;

public interface IDeprecationsRepository
{
    /// <summary>
    /// Get List of deprecations by filter
    /// </summary>
    Task<DeprecationNoticesResult> GetDeprecationsAsync(DeprecationsRequestModel deprecationsRequestModel,
        CancellationToken cancellation = default);
}