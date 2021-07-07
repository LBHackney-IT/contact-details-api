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
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;

        public CreateContactUseCase(IContactDetailsGateway gateway, ISnsGateway snsGateway, ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        public async Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token)
        {
            var contact = await _gateway.CreateContact(contactRequest).ConfigureAwait(false);

            var createContactDetailsSnsMessage = _snsFactory.Create(contactRequest, token);
            await _snsGateway.Publish(createContactDetailsSnsMessage).ConfigureAwait(false);

            return contact.ToResponse();
        }
    }
}
