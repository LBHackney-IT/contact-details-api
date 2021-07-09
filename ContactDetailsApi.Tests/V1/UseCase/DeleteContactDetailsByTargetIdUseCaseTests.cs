using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Xunit;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class DeleteContactDetailsByTargetIdUseCaseTests : LogCallAspectFixture
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly Mock<ISnsFactory> _mockSnsFactory;
        private readonly Mock<ISnsGateway> _mockSnsGateway;
        private readonly DeleteContactDetailsByTargetIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public DeleteContactDetailsByTargetIdUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _mockSnsFactory = new Mock<ISnsFactory>();
            _mockSnsGateway = new Mock<ISnsGateway>();

            _classUnderTest = new DeleteContactDetailsByTargetIdUseCase(_mockGateway.Object, _mockSnsFactory.Object, _mockSnsGateway.Object);
        }

        [Fact]
        public async Task DeleteByIdUseCaseShouldBeNull()
        {
            var queryParam = new DeleteContactQueryParameter
            {
                TargetId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };
            _mockGateway.Setup(x => x.DeleteContactDetailsById(queryParam)).ReturnsAsync((ContactDetails) null);

            var response = await _classUnderTest.Execute(queryParam, It.IsAny<Token>(), It.IsAny<string>()).ConfigureAwait(false);
            response.Should().BeNull();
        }

        [Fact]
        public async Task DeleteContactByIdReturnsOkResponse()
        {
            var queryParam = new DeleteContactQueryParameter
            {
                TargetId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };
            var contact = _fixture.Create<ContactDetails>();
            _mockGateway.Setup(x => x.DeleteContactDetailsById(queryParam)).ReturnsAsync(contact);

            var response = await _classUnderTest.Execute(queryParam, It.IsAny<Token>(), It.IsAny<string>()).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Fact]
        public async Task DeleteContactByIdPublishesMessage()
        {
            var queryParam = new DeleteContactQueryParameter
            {
                TargetId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };
            var contact = _fixture.Create<ContactDetails>();
            _mockGateway.Setup(x => x.DeleteContactDetailsById(queryParam)).ReturnsAsync(contact);

            var response = await _classUnderTest.Execute(queryParam, It.IsAny<Token>(), It.IsAny<string>()).ConfigureAwait(false);


            _mockSnsFactory.Verify(x => x.Create(It.IsAny<ContactDetails>(), It.IsAny<Token>(), It.IsAny<string>()));
            _mockSnsGateway.Verify(x => x.Publish(It.IsAny<ContactDetails>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void DeleteContactByIdThrowsException()
        {
            var queryParam = new DeleteContactQueryParameter
            {
                TargetId = Guid.NewGuid(),
                Id = Guid.NewGuid()
            };
            var exception = new ApplicationException("Test Exception");
            _mockGateway.Setup(x => x.DeleteContactDetailsById(queryParam)).ThrowsAsync(exception);

            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.Execute(queryParam, It.IsAny<Token>(), It.IsAny<string>()).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
