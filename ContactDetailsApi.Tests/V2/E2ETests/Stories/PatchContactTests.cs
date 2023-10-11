using Bogus.DataSets;
using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V2.E2ETests.Steps;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V2.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to update an existing contact",
        SoThat = "I can be sure contact details are up to date")]
    [Collection("AppTest collection")]
    public class PatchContactTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly PatchContactSteps _steps;

        public PatchContactTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new PatchContactSteps(appFactory.Client);
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
                _contactDetailsFixture?.Dispose();
                _snsFixture?.PurgeAllQueueMessages();

                _disposed = true;
            }
        }

        [Fact]
        public void ServiceReturns404NotFound()
        {
            this.Given(g => _contactDetailsFixture.GivenAPatchContactRequest(null))
                .When(w => _steps.WhenThePatchContactEndpointIsCalled(
                    _contactDetailsFixture.PatchContactRequestObject,
                    _contactDetailsFixture.PatchContactDetailsQuery,
                    _contactDetailsFixture.ExistingContact))
                .Then(t => _steps.ThenA404NotFoundResponseIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceReturnsConflictWhenVersionMismatch()
        {
            this.Given(g => _contactDetailsFixture.GivenAContactAlreadyExists())
                .And(g => _contactDetailsFixture.GivenAPatchContactRequest(_contactDetailsFixture.ExistingContact))
                .When(w => _steps.WhenThePatchContactEndpointIsCalledWithInvalidVersion(
                    _contactDetailsFixture.PatchContactRequestObject,
                    _contactDetailsFixture.PatchContactDetailsQuery,
                    _contactDetailsFixture.ExistingContact))
                .Then(t => _steps.ThenA409ConflictResponseIsReturned())
                .BDDfy();
        }

        [Fact]
        public void ServiceUpdatesContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenAContactAlreadyExists())
                .And(x => x._contactDetailsFixture.GivenAPatchContactRequest(_contactDetailsFixture.ExistingContact))
                .When(w => _steps.WhenThePatchContactEndpointIsCalled(
                    _contactDetailsFixture.PatchContactRequestObject,
                    _contactDetailsFixture.PatchContactDetailsQuery,
                    _contactDetailsFixture.ExistingContact))
                .Then(t => _steps.ThenA204NoContentResponseIsReturned())
                .Then(t => _steps.ThenTheContactDetailsAreUpdated(_contactDetailsFixture))
                .BDDfy();
        }

    }
}
