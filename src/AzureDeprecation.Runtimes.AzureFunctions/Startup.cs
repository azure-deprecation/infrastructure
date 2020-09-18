using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace AzureDeprecation.Runtimes.AzureFunctions
{
    public abstract class DefaultStartup : FunctionsStartup
    {
        public abstract string ComponentName { get; }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();

            ConfigureDependencies(builder.Services);

            var serviceProvider = builder.Services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName(ComponentName)
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

        protected virtual void ConfigureDependencies(IServiceCollection builderServices)
        {
        }
    }
}