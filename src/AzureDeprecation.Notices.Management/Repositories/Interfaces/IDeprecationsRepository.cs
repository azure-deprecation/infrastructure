using AzureDeprecation.Contracts.v1.Documents;

namespace AzureDeprecation.Notices.Management.Repositories.Interfaces
{
    public interface IDeprecationsRepository
    {
        /// <summary>
        /// Determines if a deprecation notice exist
        /// </summary>
        public Task<bool> DoesDeprecationExistAsync(string deprecationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Store a new deprecation notice
        /// </summary>
        /// <param name="deprecationNoticeDocument">Information concerning the deprecation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task PersistDeprecationAsync(DeprecationNoticeDocument deprecationNoticeDocument, CancellationToken cancellationToken = default);
    }
}
