using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.UseCase;
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
    public class ServicesoftController : ControllerBase
    {
        private readonly IFetchAllContactDetailsByUprnUseCase _fetchAllContactDetailsByUprnUseCase;

        public ServicesoftController(IFetchAllContactDetailsByUprnUseCase fetchAllContactDetailsByUprnUseCase)
        {
            _fetchAllContactDetailsByUprnUseCase = fetchAllContactDetailsByUprnUseCase;
        }


        public async Task<IActionResult> FetchAllContactDetailsByUprn()
        {
                var results = await _fetchAllContactDetailsByUprnUseCase.ExecuteAsync();

            return Ok(results);
        }
    }
}
