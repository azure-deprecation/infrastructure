using AzureDeprecation.APIs.REST;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureDeprecation.APIs.REST
{
    public class Startup : DefaultStartup
    {
        public override string ComponentName { get; } = "REST API";
    }
}
