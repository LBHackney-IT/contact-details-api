using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace ContactDetailsApi.Tests.V1.E2ETests.Stories
{
    [Story(
        AsA = "Internal Hackney user (such as a Housing Officer or Area housing Manager)",
        IWant = "to be able to delete contact details",
        SoThat = "I am aware of the the Person’s most up to date contact information")]
    [Collection("DynamoDb collection")]
    public class DeleteContactDetailsByTargetIdTests : IDisposable
    {
        private readonly AwsIntegrationTests<Startup> _dbFixture;
        private readonly ContactDetailsFixture _contactDetailsFixture;
        private readonly DeleteContactDetailsSteps _steps;

        public DeleteContactDetailsByTargetIdTests(AwsIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
            _contactDetailsFixture = new ContactDetailsFixture(_dbFixture.DynamoDbContext);
            _steps = new DeleteContactDetailsSteps(_dbFixture.Client);
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
        public void ServiceSoftDeletesRequestedContactDetails()
        {
            this.Given(g => _contactDetailsFixture.GivenContactDetailsAlreadyExist(1, 0))
                .When(w => _steps.WhenTheDeleteContactDetailsApiIsCalled(_contactDetailsFixture.TargetId.ToString(), _contactDetailsFixture.Contacts.First().Id.ToString()))
                .Then(t => _steps.ThenTheContactDetailsAreDeleted(_contactDetailsFixture))
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
