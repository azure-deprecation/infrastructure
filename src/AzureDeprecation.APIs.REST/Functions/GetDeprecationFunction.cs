using System.Net;
using System.Web.Http;
using AutoMapper;
using AzureDeprecation.APIs.REST.DataAccess.Interfaces;
using AzureDeprecation.APIs.REST.DataAccess.Models;
using AzureDeprecation.Contracts.v1.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Presentation = AzureDeprecation.APIs.REST.Contracts;

namespace AzureDeprecation.APIs.REST.Functions;

public class GetDeprecationFunction
{
    readonly IDeprecationsRepository _deprecationsRepository;
    readonly IMapper _mapper;

    public GetDeprecationFunction(
        IDeprecationsRepository deprecationsRepository,
        IMapper mapper)
    {
        _deprecationsRepository = deprecationsRepository;
        _mapper = mapper;
    }

    [FunctionName("get-deprecation")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/deprecations/{id}")]
        HttpRequest request,
        string id,
        ILogger log,
        CancellationToken cancellationToken = default)
    {
        Presentation.DeprecationInfo result;
        
        try
        {
            var deprecation = await _deprecationsRepository.GetDeprecationAsync(id, cancellationToken);
            result = _mapper.Map<Presentation.DeprecationInfo>(deprecation);
        }
        catch (Presentation.ServiceException exception) when (exception.HttpStatusCode == HttpStatusCode.NotFound)
        {
            return new NotFoundResult();
        }
        catch
        {
            return new InternalServerErrorResult();
        }

        return new OkObjectResult(result);
    }
}