using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Controllers
{
    [ApiController]
    [Route("api/v2/contactDetails")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ContactDetailsController : BaseController
    {
        private readonly ICreateContactUseCase _createContactUseCase;
        private readonly IHttpContextWrapper _httpContextWrapper;
        private readonly ITokenFactory _tokenFactory;

        public ContactDetailsController(
            ICreateContactUseCase createContactUseCase,
            IHttpContextWrapper httpContextWrapper,
            ITokenFactory tokenFactory)
        {
            _createContactUseCase = createContactUseCase;
            _httpContextWrapper = httpContextWrapper;
            _tokenFactory = tokenFactory;
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
            var token = _tokenFactory.Create(_httpContextWrapper.GetContextRequestHeaders(HttpContext));

            var result = await _createContactUseCase.ExecuteAsync(contactRequest, token).ConfigureAwait(false);

            // Reminder to change URL to /v2
            return Created("api/v1/contactDetails", result);
        }
    }
}
