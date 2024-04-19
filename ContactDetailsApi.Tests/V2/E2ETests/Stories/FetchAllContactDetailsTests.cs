using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V2.E2ETests.Steps;
using Hackney.Core.Testing.DynamoDb;
using System;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V2.E2ETests.Stories
{
    [Story(
        AsA = "External ServiceSoft user",
        IWant = "to be able to fetch all the contacts details of Hackney tenants",
        SoThat = "I can use this to update data in my servicesoft application")]
    [Collection("AppTest collection")]
    public class FetchAllContactDetailsTests : IDisposable
    {

        private readonly IDynamoDbFixture _dbFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly FetchAllContactDetailsByUprnStep _steps;

        public FetchAllContactDetailsTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new FetchAllContactDetailsByUprnStep(appFactory.Client);
            Environment.SetEnvironmentVariable("AUTH_ALLOWED_GROUPS_EXTERNAL", "e2e-testing");

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
                _dbFixture?.Dispose();
                _contactDetailsFixture?.Dispose();

                _disposed = true;
            }
        }


        [Fact]
        public void ServiceReturnsAllContactDetailsAsRequested()
        {
            this.Given(g => _contactDetailsFixture.GivenAFetchAllContactDetailsByUprnRequest())
                .When(w => _steps.WhenAllContactDetailsAreRequested())
                .Then(t => _steps.ThenAllContactDetailsAreReturned(_dbFixture))
                .BDDfy();
        }

        //[Fact]
        //public void ServiceReturnsUnauthorizedWhenUserIsNotInAllowedGroups()
        //{
        //    Environment.SetEnvironmentVariable("AUTH_ALLOWED_GROUPS_EXTERNAL", "unauthorized-group");

        //    this.Given(g => _contactDetailsFixture.GivenAFetchAllContactDetailsByUprnRequest())
        //        .When(w => _steps.WhenAllContactDetailsAreRequested())
        //        .Then(t => _steps.ThenUnauthorizedIsReturned())
        //        .BDDfy();

        //}
    }
}
