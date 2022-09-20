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
            builder.AddHttpCorrelation();

            ConfigureDependencies(builder.Services);

            var config = builder.GetContext().Configuration;
            var instrumentationKey = config.GetValue<string>("APPINSIGHTS_INSTRUMENTATIONKEY");

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithComponentName(ComponentName)
                .Enrich.WithVersion()
                .WriteTo.Console();

            if (string.IsNullOrWhiteSpace(instrumentationKey) == false)
            {
                loggerConfiguration.WriteTo.AzureApplicationInsightsWithInstrumentationKey(instrumentationKey);
            }

            var logger = loggerConfiguration.CreateLogger();

            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.RemoveMicrosoftApplicationInsightsLoggerProvider();
                loggingBuilder.AddSerilog(logger, dispose: true);
            });
        }

        protected virtual void ConfigureDependencies(IServiceCollection builderServices)
        {
        }
    }
}