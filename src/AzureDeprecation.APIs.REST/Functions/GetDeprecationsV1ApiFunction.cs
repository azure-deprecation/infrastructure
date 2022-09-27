using System.Net;
using AutoMapper;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.Utils;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Presentation = AzureDeprecation.APIs.REST.Contracts;

namespace AzureDeprecation.APIs.REST.Functions
{
    public partial class GetDeprecationsV1ApiFunction
    {
        readonly IDeprecationsRepository _deprecationsRepository;
        readonly ILogger<GetDeprecationsV1ApiFunction> _logger;
        readonly IMapper _mapper;

        public GetDeprecationsV1ApiFunction(
            IDeprecationsRepository deprecationsRepository,
            IMapper mapper,
            ILogger<GetDeprecationsV1ApiFunction> logger)
        {
            _deprecationsRepository = deprecationsRepository;
            _logger = logger;
            _mapper = mapper;
        }
        
        [FunctionName("apis-v1-get-deprecations")]
        [OpenApiOperation("GetDeprecations", tags: "deprecations", Summary = "Get all deprecations", Description = "Provides capability to browse all deprecations")]
        [OpenApiParameter("filters.status", In = ParameterLocation.Query, Required = false, Description = "Filter to reduce deprecation notices by a given status.")]
        [OpenApiParameter("filters.year", In = ParameterLocation.Query, Required = false, Description = "Filter to reduce deprecation notices by the year of the deprecation.")]
        [OpenApiParameter("filters.service", In = ParameterLocation.Query, Required = false, Description = "Filter to reduce deprecation notices for a given Azure service.")]
        [OpenApiParameter("filters.impactType", In = ParameterLocation.Query, Description = "Filter to reduce deprecation notices by a given impact type.")]
        [OpenApiParameter("filters.cloud", In = ParameterLocation.Query, Required = false, Description = "Filter to reduce deprecation notices for a given cloud.")]
        [OpenApiParameter("pagination.offset", In = ParameterLocation.Query, Required = false, Description = "Specifies the amount of pages to skip.")]
        [OpenApiParameter("pagination.limit", In = ParameterLocation.Query, Required = false, Description = "Specifies the amount of entries in the page.")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Presentation.DeprecationNoticesResponse))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/deprecations")] HttpRequest request,
            ILogger log,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = ValueStopwatch.StartNew();
            using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
            {
                ["RequestTraceIdentifier"] = request.HttpContext.TraceIdentifier,
            });

            Presentation.DeprecationNoticesResponse result;
            try
            {
                var filter = DeprecationRequestModelBinder.CreateModel(request.Query);
                var dbModel = new DeprecationNoticesResult();

                await foreach (var entity in _deprecationsRepository.GetDeprecationsAsync(filter, cancellationToken))
                {
                    dbModel.Deprecations.Add(entity);
                }

                result = _mapper.Map<Presentation.DeprecationNoticesResponse>(dbModel);
            }
            finally
            {
                LogTiming(stopwatch.GetElapsedTotalMilliseconds());
            }

            return new OkObjectResult(result);
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);
    }
}