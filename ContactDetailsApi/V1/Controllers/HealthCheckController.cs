using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ContactDetailsApi.V1.Controllers
{
    [Route("api/v1/healthcheck")]
    [ApiController]
    [Produces("application/json")]
    public class HealthCheckController : BaseController
    {
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), 200)]
        public IActionResult HealthCheck()
        {
            var result = new Dictionary<string, bool> { { "success", true } };

            return Ok(result);
        }
    }
}
