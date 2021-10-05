using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using System;
using System.Threading.Tasks;

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

        public async Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest,
            Token token)
        {
            var dbObject = contactRequest.ToDomain(token).ToDatabase();
            var contact = await _gateway.CreateContact(dbObject).ConfigureAwait(false);

            var contactTopicArn = Environment.GetEnvironmentVariable("CONTACT_DETAILS_SNS_ARN");

            var createContactDetailsSnsMessage = _snsFactory.Create(contact, token, EventConstants.CREATED);

            await _snsGateway.Publish(createContactDetailsSnsMessage, contactTopicArn).ConfigureAwait(false);

            return contact.ToResponse();
        }
    }
}
