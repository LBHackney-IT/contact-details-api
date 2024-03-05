using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V2.E2ETests.Steps;
using Hackney.Core.Testing.DynamoDb;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V2.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to view all relevant details about a person in one place ",
        SoThat = "I am aware of the the Person’s most up to date information and can take appropriate action")]
    [Collection("AppTest collection")]
    public class GetContactDetailsByTargetIdTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly GetContactDetailsSteps _steps;

        public GetContactDetailsByTargetIdTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new GetContactDetailsSteps(appFactory.Client);
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

        [Theory]
        [InlineData(false, 5, 0)]
        [InlineData(false, 5, 5)]
        [InlineData(false, 0, 5)]
        [InlineData(true, 5, 0)]
        [InlineData(true, 5, 5)]
        [InlineData(true, 0, 5)]
        public void ServiceReturnsTheRequestedContactDetails(bool includeHistoric, int activeCount, int inactiveCount)
        {
            this.Given(g => _contactDetailsFixture.GivenContactDetailsAlreadyExist(activeCount, inactiveCount))
                .When(w => _steps.WhenTheContactDetailsAreRequested(_contactDetailsFixture.TargetId.ToString(), includeHistoric))
                .Then(t => _steps.ThenTheContactDetailsAreReturned(_contactDetailsFixture.Contacts, includeHistoric))
                .BDDfy();
        }
    }
}
