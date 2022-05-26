using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V1.E2ETests.Stories
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
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext, _snsFixture.SimpleNotificationService);
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
                _contactDetailsFixture.?Dispose();
                _snsFixture?.PurgeAllQueueMessages();

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturnsTheRequestedContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenANewContactRequest())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.Contact))
                .Then(t => _steps.ThenTheContactDetailsAreSavedAndReturned(_contactDetailsFixture))
                .Then(t => _steps.ThenTheContactDetailsCreatedEventIsRaised(_snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenAnInvalidNewContactRequest())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.Contact))
                .Then(t => _steps.ThenBadRequestIsReturned())
                .And(t => _steps.ThenTheResponseIncludesValidationErrors())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidPhoneNumber()
        {
            this.Given(g => _contactDetailsFixture.GivenAnNewContactRequestWithAnInvalidPhoneNumber())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.Contact))
                .Then(t => _steps.ThenBadRequestValidationErrorResultIsReturned("Value", ErrorCodes.InvalidPhoneNumber))
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsBadRequestWithInvalidEmail()
        {
            this.Given(g => _contactDetailsFixture.GivenAnNewContactRequestWithAnInvalidEmail())
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.Contact))
                .Then(t => _steps.ThenBadRequestValidationErrorResultIsReturned("Value", ErrorCodes.InvalidEmail))
                .BDDfy();
        }
    }
}
