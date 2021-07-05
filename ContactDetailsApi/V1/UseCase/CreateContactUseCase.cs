using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.UseCase
{
    public class CreateContactUseCase : ICreateContactUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        public CreateContactUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token)
        {
            var contact = await _gateway.CreateContact(contactRequest).ConfigureAwait(false);

            return contact.ToResponse();
        }
    }
}
