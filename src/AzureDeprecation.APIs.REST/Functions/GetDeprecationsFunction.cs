using AutoMapper;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.APIs.REST.Utils;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using Presentation = AzureDeprecation.APIs.REST.Contracts;

namespace AzureDeprecation.APIs.REST.Functions
{
    public partial class GetDeprecationsFunction
    {
        readonly IDeprecationsRepository _deprecationsRepository;
        readonly ILogger<GetDeprecationsFunction> _logger;
        readonly IMapper _mapper;

        public GetDeprecationsFunction(
            IDeprecationsRepository deprecationsRepository,
            IMapper mapper,
            ILogger<GetDeprecationsFunction> logger)
        {
            _deprecationsRepository = deprecationsRepository;
            _logger = logger;
            _mapper = mapper;
        }
        
        [FunctionName("apis-get-deprecations")]
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