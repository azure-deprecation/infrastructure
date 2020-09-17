using AzureDeprecation.Integrations.GitHub.Repositories;
using AzureDeprecation.Notices.Management.MessageHandlers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(AzureDeprecation.Notices.Management.Startup))]
namespace AzureDeprecation.Notices.Management
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<GitHubRepository>();
            builder.Services.AddScoped<NewAzureDeprecationNotificationV1MessageHandler>();

            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName("Azure Deprecation Notice Management")
                .Enrich.WithVersion()
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsights(instrumentationKey)
                .CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProvidersExceptFunctionProviders();
                loggingBuilder.AddSerilog(logger);
            });
        }
    }
}