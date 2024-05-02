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
    public abstract class FetchAllContactDetailsTests : IDisposable
    {
        protected readonly IDynamoDbFixture _dbFixture;
        protected readonly ContactDetailsFixture _contactDetailsFixture;
        protected readonly FetchAllContactDetailsByUprnStep _steps;


        public FetchAllContactDetailsTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new FetchAllContactDetailsByUprnStep(appFactory.Client);

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
    }

    [Collection("AppTest middleware collection")]
    public class FetchAllContactDetailsTestsAuthorised : FetchAllContactDetailsTests
    {
        public FetchAllContactDetailsTestsAuthorised(
            MockWebApplicationFactoryWithMiddleware<Startup> appFactory)
        : base(appFactory)
        {
        }

        [Fact]
        public void ServiceReturnsAllContactDetailsAsRequested()
        {
            this.Given(g => _contactDetailsFixture.GivenAFetchAllContactDetailsByUprnRequest())
                .When(w => _steps.WhenAllContactDetailsAreRequested())
                .Then(t => _steps.ThenAllContactDetailsAreReturned(_dbFixture))
                .BDDfy();
        }
    }

    [Collection("AppTest collection")]
    public class FetchAllContactDetailsTestsUnauthorised : FetchAllContactDetailsTests
    {
        public FetchAllContactDetailsTestsUnauthorised(
            MockWebApplicationFactory<Startup> appFactory)
        : base(appFactory)
        {
        }

        [Fact]
        public void ServiceReturns401Unauthorised()
        {
            this.Given(g => _contactDetailsFixture.GivenAFetchAllContactDetailsByUprnRequest())
                .When(w => _steps.WhenAllContactDetailsAreRequested())
                .Then(t => _steps.ThenAnUnauthorisedResponseIsReturned())
                .BDDfy();
        }
    }

}
