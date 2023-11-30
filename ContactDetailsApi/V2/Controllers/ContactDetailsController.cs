using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using FluentValidation;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Middleware;
using Hackney.Core.Validation.AspNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ContactDetailsRequestObject = ContactDetailsApi.V2.Boundary.Request.ContactDetailsRequestObject;
using HeaderConstants = ContactDetailsApi.V2.Infrastructure.HeaderConstants;


namespace ContactDetailsApi.V2.Controllers
{
    [ApiController]
    [Route("api/v2/contactDetails")]
    [Produces("application/json")]
    [ApiVersion("1.0")]
    public class ContactDetailsController : BaseController
    {
        private readonly ICreateContactUseCase _createContactUseCase;
        private readonly IGetContactDetailsByTargetIdUseCase _getContactDetailsByTargetIdUseCase;
        private readonly IEditContactDetailsUseCase _editContactDetailsUseCase;
        private readonly IHttpContextWrapper _httpContextWrapper;
        private readonly ITokenFactory _tokenFactory;

        public ContactDetailsController(
            ICreateContactUseCase createContactUseCase,
            IGetContactDetailsByTargetIdUseCase getContactDetailsByTargetIdUseCase,
            IHttpContextWrapper httpContextWrapper,
            ITokenFactory tokenFactory,
            IEditContactDetailsUseCase editContactDetailsUseCase)
        {
            _createContactUseCase = createContactUseCase;
            _getContactDetailsByTargetIdUseCase = getContactDetailsByTargetIdUseCase;
            _httpContextWrapper = httpContextWrapper;
            _tokenFactory = tokenFactory;
            _editContactDetailsUseCase = editContactDetailsUseCase;
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
            try
            {
                var token = _tokenFactory.Create(_httpContextWrapper.GetContextRequestHeaders(HttpContext));

                var result = await _createContactUseCase.ExecuteAsync(contactRequest, token).ConfigureAwait(false);

                return Created("api/v2/contactDetails", result);
            }
            catch (ValidationException e)
            {
                return BadRequest(e.ConstructResponse());
            }
        }

        [HttpPatch]
        [Route("{contactDetailId}/person/{personId}")]
        [LogCall(LogLevel.Information)]
        public async Task<IActionResult> PatchContact([FromRoute] EditContactDetailsQuery query, [FromBody] EditContactDetailsRequest request)
        {
            var bodyText = await HttpContext.Request.GetRawBodyStringAsync().ConfigureAwait(false);
            var ifMatch = GetIfMatchFromHeader();
            var token = _tokenFactory.Create(_httpContextWrapper.GetContextRequestHeaders(HttpContext));

            try
            {
                var result = await _editContactDetailsUseCase.ExecuteAsync(query, request, bodyText, token, ifMatch).ConfigureAwait(false);
                if (result == null) return NotFound(query);

                return NoContent();
            }
            catch (VersionNumberConflictException e)
            {
                return Conflict(e.Message);
            }

        }

        private int? GetIfMatchFromHeader()
        {
            var header = HttpContext.Request.Headers.GetHeaderValue(HeaderConstants.IfMatch);

            int numericValue;

            if (header == null)
                return null;

            if (header.GetType() == typeof(string))
            {
                if (int.TryParse(header, out numericValue))
                    return numericValue;
            }

            _ = EntityTagHeaderValue.TryParse(header, out var entityTagHeaderValue);

            if (entityTagHeaderValue == null)
                return null;

            var version = entityTagHeaderValue.Tag.Replace("\"", string.Empty);

            if (int.TryParse(version, out numericValue))
                return numericValue;

            return null;
        }
    }
}
