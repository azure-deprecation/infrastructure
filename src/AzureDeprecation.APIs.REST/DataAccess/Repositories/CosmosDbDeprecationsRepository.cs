using System.Net;
using System.Runtime.CompilerServices;
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
    readonly CosmosClient _client;
    readonly CosmosDbOptions _dbOptions;
    readonly ILogger<CosmosDbDeprecationsRepository> _logger;
    
    readonly SemaphoreSlim _locker = new(1, 1);
    
    static bool _isDatabaseInitialized;

    public CosmosDbDeprecationsRepository(
        CosmosClient cosmosClient,
        IOptions<CosmosDbOptions> dbSettings,
        ILogger<CosmosDbDeprecationsRepository> logger)
    {
        Code.NotNull(dbSettings?.Value, nameof(dbSettings));
        Code.NotNullNorEmpty(dbSettings.Value.ConnectionString, nameof(dbSettings.Value.ConnectionString));

        _client = cosmosClient;
        _dbOptions = dbSettings.Value;
        _logger = logger;
    }

    public async IAsyncEnumerable<NoticeEntity> GetDeprecationsAsync(
        DeprecationsRequestModel deprecationsRequestModel,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await EnsureDatabaseInitializedAsync(cancellation);

        var db = _client.GetDatabase(_dbOptions.DatabaseName);
        var container = db.GetContainer(_dbOptions.ContainerName);

        var queryBuilder = container
            .GetItemLinqQueryable<NoticeEntity>()
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

    async Task EnsureDatabaseInitializedAsync(CancellationToken cancellationToken)
    {
        if (_isDatabaseInitialized)
            return;

        try
        {
            await _locker.WaitAsync(cancellationToken);
            if (_isDatabaseInitialized)
                return;

            var db = await _client.CreateDatabaseIfNotExistsAsync(_dbOptions.DatabaseName,
                cancellationToken: cancellationToken);
            
            _logger.LogInformation("Database {DbName} created or existed before", _dbOptions.DatabaseName);
            
            await db.Database.CreateContainerIfNotExistsAsync(_dbOptions.ContainerName,
                $"/{nameof(NoticeEntity.Id).ToLower()}", cancellationToken: cancellationToken);
            
            
            _isDatabaseInitialized = true;
        }
        finally
        {
            _locker.Release();
        }
    }
}