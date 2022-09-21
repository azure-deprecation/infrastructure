using AzureDeprecation.Notifications;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AzureDeprecation.Notifications
{
    public class Startup : DefaultStartup
    {
        public override string ComponentName { get; } = "Notifications";
    }
}