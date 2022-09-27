using AzureDeprecation.APIs.REST;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Repositories;
using AzureDeprecation.Integrations.Azure.CosmosDb.Configuration;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureDeprecation.APIs.REST;

public class Startup : DefaultStartup
{
    public override string ComponentName { get; } = "REST API";

    protected override void ConfigureDependencies(IServiceCollection services)
    {
        base.ConfigureDependencies(services);

        services.AddAutoMapper(typeof(Startup));
        services.AddCosmosDbClient();

        services.AddOptions<CosmosDbOptions>().Configure<IConfiguration>((dbSettings, config) =>
        {
            dbSettings.ConnectionString = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ConnectionString)}");
            dbSettings.DatabaseName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.DatabaseName)}");
            dbSettings.ContainerName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ContainerName)}");
        });

        services.AddTransient<IDeprecationsRepository, AzureCosmosDbDeprecationsRepository>();
    }
}