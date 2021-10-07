using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V2.E2ETests.Steps;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V2.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to create a new contact ",
        SoThat = "I am able to add contacts when the need rises")]
    [Collection("Aws collection")]
    public class CreateContactTests : IDisposable
    {
        private readonly AwsIntegrationTests<Startup> _dbFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly CreateContactSteps _steps;

        public CreateContactTests(AwsIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext, _dbFixture.SimpleNotificationService);
            _steps = new CreateContactSteps(_dbFixture.Client);
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

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequest())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenTheContactDetailsAreSavedAndReturned(_contactDetailsFixture))
                .Then(t => _steps.ThenTheContactDetailsCreatedEventIsRaised(_dbFixture.SnsVerifer))
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

        [Fact]
        public void ServiceDoesntValidateAddressLine1WhenContactTypeNotAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidAddressLine1WhenTheContactTypeNotAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenThereIsNoValidationErrorForField("AddressLine1"))
                .BDDfy();
        }

        [Fact]
        public void ServiceDoesntValidatePostCodeWhenContactTypeNotAddress()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequestWithInvalidPostCodeWhenTheContactTypeNotAddress())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.ContactRequestObject))
                .Then(t => _steps.ThenThereIsNoValidationErrorForField("PostCode"))
                .BDDfy();
        }

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
