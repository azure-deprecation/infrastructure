using System.Runtime.CompilerServices;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Utils;
using AzureDeprecation.Contracts.v1.Documents;
using AzureDeprecation.Integrations.Azure.CosmosDb.Configuration;
using AzureDeprecation.Integrations.Azure.CosmosDb.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureDeprecation.APIs.REST.DataAccess.Repositories;

internal class AzureCosmosDbDeprecationsRepository : AzureCosmosDbRepository, IDeprecationsRepository
{
    readonly ILogger<AzureCosmosDbDeprecationsRepository> _logger;

    public AzureCosmosDbDeprecationsRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> dbSettings,
        ILogger<AzureCosmosDbDeprecationsRepository> logger)
        : base(cosmosClient, dbSettings)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<DeprecationNoticeDocument> GetDeprecationsAsync(
        DeprecationsRequestModel deprecationsRequestModel,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var container = await GetContainerAsync(cancellation);

        var queryBuilder = container
            .GetItemLinqQueryable<DeprecationNoticeDocument>()
            .WithFilters(deprecationsRequestModel.Filters)
            .WithPagination(deprecationsRequestModel.Pagination);

        using var queryIterator = queryBuilder.ToFeedIterator();

        while (queryIterator.HasMoreResults)
        {
            foreach (var entity in await queryIterator.ReadNextAsync(cancellation))
            {
                yield return entity;
            }
        }
    }

    public async Task<DeprecationNoticeDocument> GetDeprecationAsync(string id, CancellationToken cancellation)
    {
        var container = await GetContainerAsync(cancellation);

        try
        {
            return await container
                .ReadItemAsync<DeprecationNoticeDocument>(id, new PartitionKey(id), cancellationToken: cancellation);
        }
        catch (CosmosException ex)
        {
            throw new ServiceException
            {
                HttpStatusCode = ex.StatusCode
            };
        }
    }
}