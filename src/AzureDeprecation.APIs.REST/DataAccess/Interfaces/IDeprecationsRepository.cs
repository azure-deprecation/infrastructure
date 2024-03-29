﻿using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.Contracts.v1.Documents;

namespace AzureDeprecation.APIs.REST.DataAccess.Interfaces
{
    public interface IDeprecationsRepository
    {
        /// <summary>
        /// Get List of deprecations by filter
        /// </summary>
        IAsyncEnumerable<DeprecationNoticeDocument> GetDeprecationsAsync(DeprecationsRequestModel deprecationsRequestModel,
            CancellationToken cancellation = default);

        /// <summary>
        /// Get deprecations by its id
        /// </summary>
        Task<DeprecationNoticeDocument> GetDeprecationAsync(string id, CancellationToken cancellation = default);
    }
}