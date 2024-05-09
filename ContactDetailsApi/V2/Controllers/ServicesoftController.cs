using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Request;

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
        [Route("contactDetails")]
        public async Task<IActionResult> FetchAllContactDetailsByUprn([FromQuery] ServicesoftFetchContactDetailsRequest request)
        {
            var results = await _fetchAllContactDetailsByUprnUseCase.ExecuteAsync(request);

            return Ok(results);
        }
    }
}
