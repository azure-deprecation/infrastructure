using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OpenAPI specs for REST endpoints
        /// </summary>
        public static IServiceCollection AddOpenApiSpecs(this IServiceCollection services)
        {
            services.AddSingleton<IOpenApiConfigurationOptions>(_ =>
            {
                var options = new OpenApiConfigurationOptions()
                {
                    Info = new OpenApiInfo
                    {
                        Version = "1.0.0",
                        Title = "Azure Deprecation API",
                        Description = "APIs to explore the deprecations in Microsoft Azure.",
                        Contact = new OpenApiContact
                        {
                            Url = new Uri("https://github.com/azure-deprecation/infrastructure")
                        },
                        License = new OpenApiLicense
                        {
                            Name = "MIT",
                            Url = new Uri("https://github.com/azure-deprecation/infrastructure/blob/main/LICENSE")
                        }
                    },
                    Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
                    OpenApiVersion = OpenApiVersionType.V2,
                    IncludeRequestingHostName = true,
                    ForceHttps = false,
                    ForceHttp = false,
                };

                return options;
            });

            return services;
        }
    }
}
