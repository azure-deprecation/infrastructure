using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AzureDeprecation.APIs.REST.DataAccess.Contracts;
using AzureDeprecation.APIs.REST.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Presentation = AzureDeprecation.APIs.REST.Contracts;

namespace AzureDeprecation.APIs.REST.Functions
{
    public class GetDeprecationsFunction
    {
        readonly IDeprecationsRepository _deprecationsRepository;
        readonly IMapper _mapper;

        public GetDeprecationsFunction(
            IDeprecationsRepository deprecationsRepository,
            IMapper mapper)
        {
            _deprecationsRepository = deprecationsRepository;
            _mapper = mapper;
        }
        
        [FunctionName("get-deprecations")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "api/v1/deprecations")] HttpRequest request,
            ILogger log,
            CancellationToken cancellationToken = default)
        {
            var filter = DeprecationRequestModelBinder.CreateModel(request.Query);
            var entities = await _deprecationsRepository.GetDeprecationsAsync(filter, cancellationToken);
            var result = _mapper.Map<Presentation.DeprecationNoticesResponse>(entities);
            return new JsonResult(result, JsonSettingsProvider.GetDefault());
        }
    }
}