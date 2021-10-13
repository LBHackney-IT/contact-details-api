using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
{
    public class CreateContactUseCase : ICreateContactUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        private readonly ISnsGateway _snsGateway;
        private readonly Factories.ISnsFactory _snsFactory;

        public CreateContactUseCase(IContactDetailsGateway gateway, ISnsGateway snsGateway, Factories.ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        [LogCall]
        public async Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token)
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
