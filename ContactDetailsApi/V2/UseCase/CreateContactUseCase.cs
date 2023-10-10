using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request.Validation;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using FluentValidation;
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
        private readonly Factories.Interfaces.ISnsFactory _snsFactory;

        public CreateContactUseCase(IContactDetailsGateway gateway, ISnsGateway snsGateway, Factories.Interfaces.ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        [LogCall]
        public async Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token)
        {
            await CheckMaximumsNotReached(contactRequest).ConfigureAwait(false);

            var dbObject = contactRequest.ToDomain(token).ToDatabase();
            var contact = await _gateway.CreateContact(dbObject).ConfigureAwait(false);

            var contactTopicArn = Environment.GetEnvironmentVariable("CONTACT_DETAILS_SNS_ARN");

            var createContactDetailsSnsMessage = _snsFactory.Create(contact, token, EventConstants.CREATED);

            await _snsGateway.Publish(createContactDetailsSnsMessage, contactTopicArn).ConfigureAwait(false);

            return contact.ToResponse();
        }

        private async Task CheckMaximumsNotReached(ContactDetailsRequestObject contactRequest)
        {
            var getExistingRequest = new V1.Boundary.Request.ContactQueryParameter()
            {
                TargetId = contactRequest.TargetId,
                IncludeHistoric = false
            };
            var existingContacts = await _gateway.GetContactDetailsByTargetId(getExistingRequest).ConfigureAwait(false);

            if (existingContacts != null)
            {
                var testObject = new CanCreateContactRequest() { ExistingContacts = existingContacts, Request = contactRequest };
                var validator = new CanCreateContactValidator();
                var validationResult = validator.Validate(testObject);
                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
