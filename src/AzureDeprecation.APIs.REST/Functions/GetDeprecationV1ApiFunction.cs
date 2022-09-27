using System.Net;
using System.Web.Http;
using AutoMapper;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.Runtimes.AzureFunctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Presentation = AzureDeprecation.APIs.REST.Contracts;

namespace AzureDeprecation.APIs.REST.Functions;

public partial class GetDeprecationV1ApiFunction
{
    readonly IDeprecationsRepository _deprecationsRepository;
    readonly ILogger<GetDeprecationV1ApiFunction> _logger;
    readonly IMapper _mapper;

    public GetDeprecationV1ApiFunction(
        IDeprecationsRepository deprecationsRepository,
        IMapper mapper,
        ILogger<GetDeprecationV1ApiFunction> logger)
    {
        _deprecationsRepository = deprecationsRepository;
        _logger = logger;
        _mapper = mapper;
    }

    [FunctionName("apis-v1-get-deprecation")]
    [OpenApiOperation("GetDeprecation", tags: "deprecations", Summary = "Get deprecation details", Description = "Provides capability to get detailed about a specific deprecation")]
    [OpenApiParameter("id", Required = true, Description = "The unique ID of the deprecation.")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(Presentation.DeprecationInfo))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/deprecations/{id}")]
        HttpRequest request,
        string id,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = ValueStopwatch.StartNew();
        using var loggerMessageScope = _logger.BeginScope(new Dictionary<string, object>()
        {
            ["RequestTraceIdentifier"] = request.HttpContext.TraceIdentifier,
        });
        Presentation.DeprecationInfo result;
        
        try
        {
            var deprecation = await _deprecationsRepository.GetDeprecationAsync(id, cancellationToken);
            result = _mapper.Map<Presentation.DeprecationInfo>(deprecation);
            LogDeprecationWasFound(id);
        }
        catch (Presentation.ServiceException exception) when (exception.HttpStatusCode == HttpStatusCode.NotFound)
        {
            LogDeprecationNotFound(id);
            return new NotFoundResult();
        }
        catch(Exception exception)
        {
            LogException(exception.Message);
            return new InternalServerErrorResult();
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

    [LoggerMessage(EventId = 201, EventName = "Timing", Level = LogLevel.Debug,
        Message = "Deprecation with ID {DeprecationId} was found.")]
    partial void LogDeprecationWasFound(string deprecationId);

    [LoggerMessage(EventId = 202, EventName = "Timing", Level = LogLevel.Debug,
        Message = "Deprecation with ID {DeprecationId} was not found.")]
    partial void LogDeprecationNotFound(string deprecationId);

    [LoggerMessage(EventId = 203, EventName = "Timing", Level = LogLevel.Debug,
        Message = "Unable to get deprecation information due to exception {exceptionMessage}.")]
    partial void LogException(string exceptionMessage);
}