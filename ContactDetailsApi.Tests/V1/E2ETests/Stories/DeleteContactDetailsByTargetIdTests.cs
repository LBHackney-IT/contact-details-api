using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using System;
using System.Linq;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to delete contact details",
        SoThat = "I am aware of the the Person’s most up to date contact information")]
    [Collection("AppTest collection")]
    public class DeleteContactDetailsByTargetIdTests : IDisposable
    {
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ISnsFixture _snsFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly DeleteContactDetailsSteps _steps;

        public DeleteContactDetailsByTargetIdTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _dbFixture = appFactory.DynamoDbFixture;
            _snsFixture = appFactory.SnsFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext, _snsFixture.SimpleNotificationService);
            _steps = new DeleteContactDetailsSteps(appFactory.Client);
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
        public void ServiceSoftDeletesRequestedContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenContactDetailsAlreadyExist(1, 0))
                .When(w => _steps.WhenTheDeleteContactDetailsApiIsCalled(_contactDetailsFixture.TargetId.ToString(), _contactDetailsFixture.Contacts.First().Id.ToString()))
                .Then(t => _steps.ThenTheContactDetailsAreDeleted(_contactDetailsFixture))
                .Then(t => _steps.ThenTheContactDetailsDeletedEventIsRaised(_contactDetailsFixture, _snsFixture))
                .BDDfy();
        }

        [Fact]
        public void ServiceSoftDeletesRequestedContactDetailsReturnNotFound()
        {
            this.Given(g => _contactDetailsFixture.GivenContactDetailsDoesNotExist())
                .When(w => _steps.WhenTheDeleteContactDetailsApiIsCalled(_contactDetailsFixture.TargetId.ToString(), Guid.NewGuid().ToString()))
                .Then(t => _steps.ThenNotFoundReturned())
                .BDDfy();
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("yhtgfsgf", "hjfhdgff")]
        [InlineData("00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        public void ServiceSoftDeletesRequestedContactDetailsReturnBadRequest(string targetId, string id)
        {
            this.Given(g => _contactDetailsFixture.GivenContactDetailsDoesNotExist())
                .When(w => _steps.WhenTheDeleteContactDetailsApiIsCalled(targetId, id))
                .Then(t => _steps.ThenBadRequestReturned())
                .BDDfy();
        }
    }
}
