using AzureDeprecation.APIs.REST;
using AzureDeprecation.APIs.REST.DataAccess.Repositories;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Contracts;
using AzureDeprecation.APIs.REST.Settings;
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
        
        services.AddOptions<CosmosDbOptions>().Configure<IConfiguration>((dbSettings, config) =>
        {
            config.GetSection(CosmosDbOptions.SectionName).Bind(dbSettings);
        });
        services.AddSingleton<IDeprecationsRepository, CosmosDbDeprecationsRepository>();
    }
}