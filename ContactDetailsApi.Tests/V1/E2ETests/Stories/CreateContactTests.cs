using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to create a new contact ",
        SoThat = "I am able to add contacts when the need rises")]
    [Collection("DynamoDb collection")]
    public class CreateContactTests : IDisposable
    {
        private readonly DynamoDbIntegrationTests<Startup> _dbFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly CreateContactSteps _steps;

        public CreateContactTests(DynamoDbIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
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
                .When(w => _steps.WhenTheCreateContactEndpointIsCalled(_contactDetailsFixture.Contact))
                .Then(t => _steps.ThenTheContactDetailsAreReturned(_contactDetailsFixture.Contact))
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
    }
}
