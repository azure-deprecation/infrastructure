using System.Net;
using System.Web.Http;
using AutoMapper;
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
        private const string StatusFilter = "status";
        private const string ImpactAreaFilter = "impactArea";
        private const string YearFilter = "deprecationYear";
        private const string AzureServiceFilter = "azureService";
        private const string ImpactTypeFilter = "impactType";
        private const string CloudFilter = "cloud";
        private const string PageOffset = "pageOffset";
        private const string PageSize = "pageSize";

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
        [OpenApiParameter(StatusFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(StatusFilter),
            Description = "Filter to reduce deprecation notices by a given status.")]
        [OpenApiParameter(ImpactAreaFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(ImpactArea),
            Description = "Filter to reduce deprecation notices for a given area of impact.")]
        [OpenApiParameter(YearFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(int),
            Description = "Filter to reduce deprecation notices by the year of the deprecation.")]
        [OpenApiParameter(AzureServiceFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(AzureService),
            Description = "Filter to reduce deprecation notices for a given Azure service.")]
        [OpenApiParameter(ImpactTypeFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(ImpactType),
            Description = "Filter to reduce deprecation notices by a given impact type.")]
        [OpenApiParameter(CloudFilter, In = ParameterLocation.Query,
            Required = false, Type = typeof(AzureCloud),
            Description = "Filter to reduce deprecation notices for a given cloud.")]
        [OpenApiParameter(PageOffset, In = ParameterLocation.Query,
            Required = false, Type = typeof(int),
            Description = "Specifies the amount of pages to skip.")]
        [OpenApiParameter(PageSize, In = ParameterLocation.Query,
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
                var requestFilter = DetermineRequestFilters(request.Query);
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
        
        DeprecationsRequestModel DetermineRequestFilters(IQueryCollection requestQuery)
        {
            return new DeprecationsRequestModel
            {
                Filters = ReadFilters(requestQuery),
                Pagination = ReadPagination(requestQuery)
            };
        }

        PaginationNoticesRequest ReadPagination(IQueryCollection requestQuery)
        {
            var paginationInfo = new PaginationNoticesRequest();

            var configuredPageLimit = GetQueryParameterForInteger(PageSize, requestQuery);
            if (configuredPageLimit != null)
            {
                paginationInfo.Limit = configuredPageLimit.Value;
            }

            var configuredPageOffset = GetQueryParameterForInteger(PageSize, requestQuery);
            if (configuredPageOffset != null)
            {
                paginationInfo.Offset = configuredPageOffset.Value;
            }

            return paginationInfo;
        }

        FilterNoticesRequest ReadFilters(IQueryCollection requestQuery)
        {
            var noticeFilters = new FilterNoticesRequest
            {
                Area = GetQueryParameterForEnum<ImpactArea>(ImpactAreaFilter, requestQuery),
                Cloud = GetQueryParameterForEnum<AzureCloud>(CloudFilter, requestQuery),
                ImpactType = GetQueryParameterForEnum<ImpactType>(ImpactTypeFilter, requestQuery),
                AzureService = GetQueryParameterForEnum<AzureService>(AzureServiceFilter, requestQuery),
                Status = GetQueryParameterForEnum<StatusFilter>(StatusFilter, requestQuery),
                Year = GetQueryParameterForInteger(YearFilter, requestQuery)
            };

            return noticeFilters;
        }

        private TEnum? GetQueryParameterForEnum<TEnum>(string queryParameterName, IQueryCollection requestQuery)
            where TEnum : struct, Enum
        {
            if (!requestQuery.ContainsKey(queryParameterName))
            {
                return null;
            }

            if (Enum.TryParse(requestQuery[queryParameterName], ignoreCase: true, out TEnum parameterValue) == false)
            {
                var allowedValues = Enum.GetValues<TEnum>();
                throw new BadHttpRequestException($"Value of {queryParameterName} is not valid. Allowed values are {string.Join(", ", allowedValues)}");
            }

            return parameterValue;
        }

        private int? GetQueryParameterForInteger(string queryParameterName, IQueryCollection requestQuery)
        {
            if (!requestQuery.ContainsKey(queryParameterName))
            {
                return null;
            }

            if (int.TryParse(requestQuery[queryParameterName], out var parameterValue) == false)
            {
                throw new BadHttpRequestException($"Value of {queryParameterName} is not a valid integer");
            }

            return parameterValue;
        }

        [LoggerMessage(EventId = 200, EventName = "Timing", Level = LogLevel.Debug,
            Message = "Timing: {ElapsedMilliseconds} ms.")]
        partial void LogTiming(double elapsedMilliseconds);
    }
}