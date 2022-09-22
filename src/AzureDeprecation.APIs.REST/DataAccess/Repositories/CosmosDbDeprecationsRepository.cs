using System.Net;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.DataAccess.Utils;
using AzureDeprecation.APIs.REST.Settings;
using CodeJam;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureDeprecation.APIs.REST.DataAccess.Repositories;

internal class CosmosDbDeprecationsRepository : IDeprecationsRepository
{
    readonly ILogger<CosmosDbDeprecationsRepository> _logger;
    readonly CosmosClient _client;
    readonly CosmosDbOptions _dbOptions;
    
    public CosmosDbDeprecationsRepository(IOptions<CosmosDbOptions> dbSettings, ILogger<CosmosDbDeprecationsRepository> logger)
    {
        Code.NotNull(dbSettings?.Value, nameof(dbSettings));
        Code.NotNullNorEmpty(dbSettings.Value.ConnectionString, nameof(dbSettings.Value.ConnectionString));
        
        _logger = logger;
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

        try
        {
            while (queryIterator.HasMoreResults)
            {
                foreach (var entity in await queryIterator.ReadNextAsync(cancellation))
                {
                    result.Add(entity);
                }
            }
        }
        catch (CosmosException ex)  when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError(ex, "Database {Db} or container {Container} not exists", _dbOptions.DbName, _dbOptions.ContainerId);
            throw new ArgumentException("Container does exist");
        }

        return new DeprecationNoticesResult
        {
            Deprecations = result
        };
    }
}