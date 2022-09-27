using AzureDeprecation.APIs.REST;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Repositories;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
#if DEBUG
        services.AddOpenApiSpecs();
#endif

        services.AddTransient<IDeprecationsRepository, AzureCosmosDbDeprecationsRepository>();
    }
}