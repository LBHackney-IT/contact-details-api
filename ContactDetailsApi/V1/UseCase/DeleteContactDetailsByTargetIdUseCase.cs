using System;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using ContactDetailsApi.V1.Domain;
using Hackney.Core.JWT;
using Hackney.Core.Sns;

namespace ContactDetailsApi.V1.UseCase
{
    public class DeleteContactDetailsByTargetIdUseCase : IDeleteContactDetailsByTargetIdUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        private readonly ISnsFactory _snsFactory;
        private readonly ISnsGateway _snsGateway;

        public DeleteContactDetailsByTargetIdUseCase(IContactDetailsGateway gateway, ISnsFactory factory, ISnsGateway snsGateway)
        {
            _gateway = gateway;
            _snsFactory = factory;
            _snsGateway = snsGateway;
        }

        [LogCall]
        public async Task<ContactDetailsResponseObject> Execute(DeleteContactQueryParameter query, Token token, string eventType)
        {
            var contact = await _gateway.DeleteContactDetailsById(query).ConfigureAwait(false);

            if (contact != null)
            {
                await PublishContact(token, eventType, contact).ConfigureAwait(false);
            }

            return contact.ToResponse();
        }

        private async Task PublishContact(Token token, string eventType, ContactDetails contact)
        {
            var contactTopicArn = Environment.GetEnvironmentVariable("CONTACT_DETAILS_SNS_ARN");

            var createContactDetailsSnsMessage = _snsFactory.Create(contact, token, eventType);

            await _snsGateway.Publish(createContactDetailsSnsMessage, contactTopicArn).ConfigureAwait(false);
        }
    }
}
