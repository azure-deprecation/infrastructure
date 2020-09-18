using AutoMapper;
using AzureDeprecation.Contracts.Messages.v1;
using AzureDeprecation.Integrations.GitHub.Repositories;
using AzureDeprecation.Notices.Management.MessageHandlers;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Octokit;

[assembly: FunctionsStartup(typeof(AzureDeprecation.Notices.Management.Startup))]
namespace AzureDeprecation.Notices.Management
{
    public class Startup : DefaultStartup
    {
        public override string ComponentName { get; } = "Notice Management";

        protected override void ConfigureDependencies(IServiceCollection services)
        {
            services.AddAutoMapper(typeof(Startup));
            services.AddScoped<GitHubRepository>();
            services.AddScoped<NewAzureDeprecationNotificationV1MessageHandler>();
        }
    }
}