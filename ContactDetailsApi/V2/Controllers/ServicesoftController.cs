using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        [HttpGet]
        [AuthorizeEndpointByGroups("AUTH_ALLOWED_GROUPS_EXTERNAL")]
        public async Task<IActionResult> FetchAllContactDetailsByUprn()
        {
            var results = await _fetchAllContactDetailsByUprnUseCase.ExecuteAsync();

            return Ok(results);
        }
    }
}
