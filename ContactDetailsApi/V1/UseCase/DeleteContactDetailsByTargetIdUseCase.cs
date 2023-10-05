using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Factories.Interfaces;
using ContactDetailsApi.V1.Gateways.Interfaces;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using System;
using System.Threading.Tasks;

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
        public async Task<ContactDetailsResponseObject> Execute(DeleteContactQueryParameter query, Token token)
        {
            var contact = await _gateway.DeleteContactDetailsById(query).ConfigureAwait(false);

            if (contact != null)
            {
                await PublishContact(token, contact).ConfigureAwait(false);
            }

            return contact.ToResponse();
        }

        private async Task PublishContact(Token token, ContactDetails contact)
        {
            var contactTopicArn = Environment.GetEnvironmentVariable("CONTACT_DETAILS_SNS_ARN");

            var createContactDetailsSnsMessage = _snsFactory.Create(contact, token, EventConstants.DELETED);

            await _snsGateway.Publish(createContactDetailsSnsMessage, contactTopicArn).ConfigureAwait(false);
        }
    }
}
