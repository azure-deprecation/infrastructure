using System.Net;
using System.Web.Http;
using AutoMapper;
using AzureDeprecation.APIs.REST.Contracts;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.Contracts.Enum;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.AspNetCore.Components;
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
        [OpenApiOperation("GetDeprecations", tags: "deprecations", Summary = "Get all deprecations",
            Description = "Provides capability to browse all deprecations")]
        [OpenApiParameter(DeprecationsRequestModel.StatusQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(StatusFilter),
            Description = "Filter to reduce deprecation notices by a given status.")]
        [OpenApiParameter(DeprecationsRequestModel.ImpactAreaQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(ImpactArea),
            Description = "Filter to reduce deprecation notices for a given area of impact.")]
        [OpenApiParameter(DeprecationsRequestModel.YearQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(int),
            Description = "Filter to reduce deprecation notices by the year of the deprecation.")]
        [OpenApiParameter(DeprecationsRequestModel.AzureServiceQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(AzureService),
            Description = "Filter to reduce deprecation notices for a given Azure service.")]
        [OpenApiParameter(DeprecationsRequestModel.ImpactTypeQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(ImpactType),
            Description = "Filter to reduce deprecation notices by a given impact type.")]
        [OpenApiParameter(DeprecationsRequestModel.CloudQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(AzureCloud),
            Description = "Filter to reduce deprecation notices for a given cloud.")]
        [OpenApiParameter(DeprecationsRequestModel.PageOffsetQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(int),
            Description = "Specifies the amount of pages to skip.")]
        [OpenApiParameter(DeprecationsRequestModel.PageSizeQueryParameterName, In = ParameterLocation.Query,
            Required = false, Type = typeof(int),
            Description = "Specifies the amount of entries in the page.")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Presentation.DeprecationNoticesResponse))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Admin, "get", Route = "api/v1/deprecations")] HttpRequest request,
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
                var requestFilter = DeprecationsRequestModel.Parse(request.Query);
                var dbModel = new DeprecationNoticesResult();

                await foreach (var entity in _deprecationsRepository.GetDeprecationsAsync(requestFilter,
                                   cancellationToken))
                {
                    dbModel.Deprecations.Add(entity);
                }

                result = _mapper.Map<Presentation.DeprecationNoticesResponse>(dbModel);
            }
            catch (BadHttpRequestException badHttpRequestException)
            {
                return new BadRequestErrorMessageResult(badHttpRequestException.Message);
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