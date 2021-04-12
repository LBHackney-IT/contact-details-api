using Amazon.XRay.Recorder.Core.Internal.Entities;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public ContactDetailsController(IGetContactByTargetIdUseCase getByIdUseCase)
        {
            _getContactByTargetIdUseCase = getByIdUseCase;
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
        public async Task<IActionResult> GetContactByTargetId([FromQuery] ContactQueryParameter cqp)
        {

            var contact = await _getContactByTargetIdUseCase.Execute(cqp).ConfigureAwait(false);
            if (null == contact || contact.Count == 0) return NotFound(cqp);

            return Ok(contact);
        }
    }
}
