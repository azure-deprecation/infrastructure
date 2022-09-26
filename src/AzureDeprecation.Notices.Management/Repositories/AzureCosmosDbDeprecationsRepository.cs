using AzureDeprecation.Integrations.Azure.CosmosDb.Configuration;
using GuardNet;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AzureDeprecation.Notices.Management.Repositories.Interfaces;
using AzureDeprecation.Integrations.Azure.CosmosDb.Repositories;
using AzureDeprecation.Contracts.v1.Documents;
using Microsoft.Azure.Cosmos.Linq;

namespace AzureDeprecation.Notices.Management.Repositories
{
    internal class AzureCosmosDbDeprecationsRepository : AzureCosmosDbRepository, IDeprecationsRepository
    {
        public AzureCosmosDbDeprecationsRepository(CosmosClient cosmosClient, IOptions<CosmosDbOptions> dbSettings)
            : base(cosmosClient, dbSettings)
        {
        }

        public async Task<bool> DoesDeprecationExistAsync(string deprecationId, CancellationToken cancellationToken)
        {
            Guard.NotNullOrWhitespace(deprecationId, nameof(deprecationId));

            var container = await GetContainerAsync(cancellationToken);

            var outcome = await container.GetItemLinqQueryable<DeprecationNoticeDocument>()
                .Where(x => x.Id == deprecationId)
                .CountAsync(cancellationToken);

            // Normally we'd expect it to be 1, but you never know if something went wrong in the past
            return outcome?.Resource > 0;
        }

        public async Task PersistDeprecationAsync(DeprecationNoticeDocument deprecationNoticeDocument, CancellationToken cancellationToken)
        {
            var container = await GetContainerAsync(cancellationToken);

            await container.UpsertItemAsync(deprecationNoticeDocument, cancellationToken: cancellationToken);
        }
    }
}
