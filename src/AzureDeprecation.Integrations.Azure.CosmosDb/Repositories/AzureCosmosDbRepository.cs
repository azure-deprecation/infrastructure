using AzureDeprecation.Integrations.Azure.CosmosDb.Configuration;
using GuardNet;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace AzureDeprecation.Integrations.Azure.CosmosDb.Repositories
{
    public class AzureCosmosDbRepository
    {
        readonly SemaphoreSlim _locker = new(1, 1);

        static bool _isDatabaseInitialized;
        readonly CosmosDbOptions _configuration;

        protected CosmosClient AzureCosmosDbClient { get; }
        protected string DatabaseName => _configuration.DatabaseName;
        protected string ContainerName => _configuration.ContainerName;

        public AzureCosmosDbRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> dbSettings)
        {
            Guard.NotNull(dbSettings.Value, nameof(dbSettings));
            Guard.NotNullOrWhitespace(dbSettings.Value.ConnectionString, nameof(dbSettings.Value.ConnectionString));

            AzureCosmosDbClient = cosmosClient;
            _configuration = dbSettings.Value;
        }

        protected async Task<Container> GetContainerAsync(CancellationToken cancellationToken)
        {
            await EnsureDatabaseInitializedAsync(cancellationToken);

            var db = AzureCosmosDbClient.GetDatabase(DatabaseName);
            var container = db.GetContainer(ContainerName);
            return container;
        }

        protected async Task EnsureDatabaseInitializedAsync(CancellationToken cancellationToken)
        {
            if (_isDatabaseInitialized)
            {
                return;
            }

            try
            {
                await _locker.WaitAsync(cancellationToken);
                if (_isDatabaseInitialized)
                {
                    return;
                }

                var db = await AzureCosmosDbClient.CreateDatabaseIfNotExistsAsync(DatabaseName, cancellationToken: cancellationToken);
                await db.Database.CreateContainerIfNotExistsAsync(ContainerName, "/id", cancellationToken: cancellationToken);

                _isDatabaseInitialized = true;
            }
            finally
            {
                _locker.Release();
            }
        }
    }
}
