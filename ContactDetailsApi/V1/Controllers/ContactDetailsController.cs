using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Logging;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Controllers
{
    [ApiController]
    [Route("api/v1/contactDetails")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ContactDetailsController : BaseController
    {
        private readonly IGetContactByTargetIdUseCase _getContactByTargetIdUseCase;
        public ContactDetailsController(IGetContactByTargetIdUseCase getByTargetIdUseCase)
        {
            _getContactByTargetIdUseCase = getByTargetIdUseCase;
        }

        /// <summary>
        /// Retrieves contact details for a particular id
        /// </summary>
        /// <response code="200">Successfully Operation</response>
        /// <response code="404">Contact not found for ID requested</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(ContactDetailsResponseObject), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> GetContactByTargetId([FromQuery] ContactQueryParameter queryParam)
        {

            var contacts = await _getContactByTargetIdUseCase.Execute(queryParam).ConfigureAwait(false);
            if (contacts == null || !contacts.Any()) return NotFound(queryParam);

            return Ok(contacts);
        }
    }
}
