// ReSharper disable once CheckNamespace

using AzureDeprecation.Integrations.Azure.CosmosDb.Configuration;
using GuardNet;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a client for Azure Cosmos DB for the configured instance, database and container.
        /// </summary>
        public static IServiceCollection AddCosmosDbClient(this IServiceCollection services)
        {
            services.AddOptions<CosmosDbOptions>().Configure<IConfiguration>((dbSettings, config) =>
            {
                dbSettings.ConnectionString = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ConnectionString)}");
                dbSettings.DatabaseName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.DatabaseName)}");
                dbSettings.ContainerName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ContainerName)}");
            });

            services.AddSingleton(sp =>
            {
                var dbSettings = sp.GetRequiredService<IOptions<CosmosDbOptions>>();
                Guard.NotNull(dbSettings.Value, nameof(CosmosDbOptions));
                Guard.NotNullOrWhitespace(dbSettings.Value.ConnectionString, nameof(dbSettings.Value.ConnectionString));

                return new CosmosClient(dbSettings.Value.ConnectionString);
            });

            return services;
        }
    }
}
