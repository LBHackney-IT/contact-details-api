using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V2.E2ETests.Steps;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain.Sns;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using System;
using TestStack.BDDfy;
using Xunit;
using ContactType = ContactDetailsApi.V1.Domain.ContactType;

namespace ContactDetailsApi.Tests.V2.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to create a new contact ",
        SoThat = "I am able to add contacts when the need rises")]
    [Collection("AppTest collection")]
    public class CreateContactTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly CreateContactSteps _steps;

        public CreateContactTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new CreateContactSteps(appFactory.Client);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != _contactDetailsFixture)
                    _contactDetailsFixture.Dispose();
                _steps.Dispose();
                _snsFixture?.PurgeAllQueueMessages();
                foreach (var action in _contactDetailsFixture._cleanup)
                    action();
                foreach (var action in _steps._cleanup)
                    action();
                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequest())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenTheContactDetailsAreSavedAndReturned(_contactDetailsFixture))
                .Then(t => _steps.ThenTheContactDetailsCreatedEventIsRaised(_snsFixture))
                .BDDfy();
        }

        [Theory]
        [InlineData(ContactType.email)]
        [InlineData(ContactType.phone)]
        public void ServiceReturnsBadRequestWithTooManyExistingContactDetails(ContactType newType)
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequest(newType))
                .And(h => _contactDetailsFixture.GivenMaxContactDetailsAlreadyExist(_contactDetailsFixture.ContactRequestObject.TargetId))
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .And(t => _steps.ThenTheResponseIncludesValidationErrorsForTooManyContacts())
                .BDDfy();
        }

        [Fact]
        public void ServiceSavesMultilineAddressToValueFieldWhenContactTypeIsAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWhereContactTypeIsAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.TheMultilineAddressIsSavedInTheValueField(_contactDetailsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceDoesntSaveMultilineAddressToValueFieldWhenContactTypeIsNotAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWhereContactTypeIsNotAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.TheMultilineAddressIsNotSavedInTheValueField(_contactDetailsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenAnInvalidNewContactRequest())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .And(t => _steps.ThenTheResponseIncludesValidationErrors())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidPhoneNumber()
        {
            this.Given(g => _contactDetailsFixture.GivenAnNewContactRequestWithAnInvalidPhoneNumber())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenBadRequestValidationErrorResultIsReturned("Value", ErrorCodes.InvalidPhoneNumber))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidEmail()
        {
            this.Given(g => _contactDetailsFixture.GivenAnNewContactRequestWithAnInvalidEmail())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenBadRequestValidationErrorResultIsReturned("Value", ErrorCodes.InvalidEmail))
                .BDDfy();
        }

        //[Fact]
        //public void ServiceDoesntValidateAddressLine1WhenContactTypeNotAddress()
        //{
        //    this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidAddressLine1WhenTheContactTypeNotAddress())
        //        .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
        //        .Then(t => _steps.ThenThereIsNoValidationErrorForField("AddressLine1"))
        //        .BDDfy();
        //}

        //[Fact]
        //public void ServiceDoesntValidatePostCodeWhenContactTypeNotAddress()
        //{
        //    this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidPostCodeWhenTheContactTypeNotAddress())
        //        .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
        //        .Then(t => _steps.ThenThereIsNoValidationErrorForField("PostCode"))
        //        .BDDfy();
        //}

        [Fact]
        public void ServiceValidatesAddressLine1WhenContactTypeIsAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidAddressLine1WhenTheContactTypeIsAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenThereIsAValidationErrorForField("AddressLine1"))
                .BDDfy();
        }

        [Fact]
        public void ServiceValidatesPostCodeWhenContactTypeIsAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidPostCodeWhenTheContactTypeIsAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenThereIsAValidationErrorForField("PostCode"))
                .BDDfy();
        }

    }
}
