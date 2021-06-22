using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
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
        private readonly IGetContactDetailsByTargetIdUseCase _getContactDetailsByTargetIdUseCase;
        private readonly ICreateContactUseCase _createContactUseCase;

        public ContactDetailsController(IGetContactDetailsByTargetIdUseCase getByTargetIdUseCase, ICreateContactUseCase createContactUseCase)
        {
            _getContactDetailsByTargetIdUseCase = getByTargetIdUseCase;
            _createContactUseCase = createContactUseCase;
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
        public async Task<IActionResult> GetContactDetailsByTargetId([FromQuery] ContactQueryParameter queryParam)
        {

            var contacts = await _getContactDetailsByTargetIdUseCase.Execute(queryParam).ConfigureAwait(false);
            if (contacts == null || !contacts.Any()) return NotFound(queryParam.TargetId);

            return Ok(new GetContactDetailsResponse(contacts));
        }

        /// <summary>
        /// Creates a new Contact
        /// </summary>
        /// <response code="201">Successfully Created</response>
        /// <response code="404">Contact not found for ID requested</response>
        /// <response code="500">Internal Server Error</response>
        [ProducesResponseType(typeof(ContactDetailsResponseObject), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> CreateContact([FromBody] ContactDetailsRequestObject contactRequest)
        {
            var result = await _createContactUseCase.ExecuteAsync(contactRequest).ConfigureAwait(false);

            return Created("api/v1/contactDetails", result);
        }
    }
}
