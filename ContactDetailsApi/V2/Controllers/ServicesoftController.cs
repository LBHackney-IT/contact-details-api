using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Hackney.Core.Authorization;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Request;
using System;

namespace ContactDetailsApi.V2.Controllers
{
    [ApiController]
    [Route("api/v2/servicesoft")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ServicesoftController : BaseController
    {
        private readonly IFetchAllContactDetailsByUprnUseCase _fetchAllContactDetailsByUprnUseCase;

        public ServicesoftController(IFetchAllContactDetailsByUprnUseCase fetchAllContactDetailsByUprnUseCase)
        {
            _fetchAllContactDetailsByUprnUseCase = fetchAllContactDetailsByUprnUseCase;
        }

        /// <summary>
        /// Retrieves contact details for a slice of tenures grouped by UPRN
        /// </summary>
        [HttpGet]
        [Route("contactDetails")]
        [AuthorizeEndpointByIpWhitelist("WHITELIST_IP_ADDRESS")]
        public async Task<IActionResult> FetchAllContactDetailsByUprn([FromQuery] ServicesoftFetchContactDetailsRequest request)
        {
            // Limit the page size to avoid API Gateway timeout (30s)
            request.PageSize = Math.Clamp(request.PageSize, 1, 500);
            var results = await _fetchAllContactDetailsByUprnUseCase.ExecuteAsync(request);

            return Ok(results);
        }
    }
}
