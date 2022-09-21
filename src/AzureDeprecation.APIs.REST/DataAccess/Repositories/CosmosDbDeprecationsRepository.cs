using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.DataAccess.Utils;
using AzureDeprecation.APIs.REST.Settings;
using CodeJam;
using CodeJam.Collections;
using CodeJam.Strings;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;

namespace AzureDeprecation.APIs.REST.DataAccess.Repositories;

internal class CosmosDbDeprecationsRepository : IDeprecationsRepository
{
    readonly CosmosClient _client;
    readonly CosmosDbOptions _dbOptions;
    
    public CosmosDbDeprecationsRepository(IOptions<CosmosDbOptions> dbSettings)
    {
        Code.NotNull(dbSettings?.Value, nameof(dbSettings));
        _dbOptions = dbSettings.Value;
        _client = new CosmosClient(dbSettings.Value.ConnectionString);
    }

    public async Task<DeprecationNoticesResult> GetDeprecationsAsync(
        DeprecationsRequestModel deprecationsRequestModel, CancellationToken cancellation = default)
    {
        var db = _client.GetDatabase(_dbOptions.DbName);
        var container = db.GetContainer(_dbOptions.ContainerId);

        var result = new List<NoticeEntity>();
        
        var queryBuilder = container
            .GetItemLinqQueryable<NoticeEntity>()
            .WithFilters(deprecationsRequestModel.Filters)
            .WithPagination(deprecationsRequestModel.Pagination);

        using var queryIterator = queryBuilder.ToFeedIterator();

        while (queryIterator.HasMoreResults)
        {
            foreach (var entity in await queryIterator.ReadNextAsync(cancellation))
            {
                result.Add(entity);
            }
        }

        return new DeprecationNoticesResult
        {
            Deprecations = result
        };
    }
}