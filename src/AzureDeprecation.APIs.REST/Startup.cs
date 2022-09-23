using AzureDeprecation.APIs.REST;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Repositories;
using AzureDeprecation.APIs.REST.Settings;
using AzureDeprecation.Runtimes.AzureFunctions;
using CodeJam;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureDeprecation.APIs.REST;

public class Startup : DefaultStartup
{
    public override string ComponentName { get; } = "REST API";

    protected override void ConfigureDependencies(IServiceCollection services)
    {
        base.ConfigureDependencies(services);

        services.AddAutoMapper(typeof(Startup));
        
        services.AddOptions<CosmosDbOptions>().Configure<IConfiguration>((dbSettings, config) =>
        {
            dbSettings.ConnectionString = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ConnectionString)}");
            dbSettings.DatabaseName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.DatabaseName)}");
            dbSettings.ContainerName = config.GetValue<string>($"{CosmosDbOptions.SectionName}_{nameof(CosmosDbOptions.ContainerName)}");
        });

        services.AddSingleton(sp =>
        {
            var dbSettings = sp.GetRequiredService<IOptions<CosmosDbOptions>>();
            Code.NotNull(dbSettings.Value, nameof(CosmosDbOptions));
            Code.NotNullNorEmpty(dbSettings.Value.ConnectionString, nameof(dbSettings.Value.ConnectionString));
            
            return new CosmosClient(dbSettings.Value.ConnectionString);
        });
        
        services.AddTransient<IDeprecationsRepository, CosmosDbDeprecationsRepository>();
    }
}