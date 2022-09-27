using AzureDeprecation.Integrations.GitHub.Repositories;
using AzureDeprecation.Notices.Management.MessageHandlers;
using AzureDeprecation.Notices.Management.Repositories;
using AzureDeprecation.Notices.Management.Repositories.Interfaces;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(AzureDeprecation.Notices.Management.Startup))]
namespace AzureDeprecation.Notices.Management
{
    public class Startup : DefaultStartup
    {
        public override string ComponentName { get; } = "Notice Management";

        protected override void ConfigureDependencies(IServiceCollection services)
        {
            base.ConfigureDependencies(services);

            services.AddAutoMapper(typeof(Startup));
            
            // Integration with Azure Cosmos DB for persistance
            services.AddCosmosDbClient();
            services.AddScoped<IDeprecationsRepository, AzureCosmosDbDeprecationsRepository>();

            // Integration with GitHub
            services.AddScoped<GitHubRepository>();

            // Message handling
            services.AddScoped<NewAzureDeprecationNotificationV1MessageHandler>();
        }
    }
}