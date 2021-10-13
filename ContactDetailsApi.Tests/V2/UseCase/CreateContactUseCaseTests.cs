using AutoFixture;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V2.UseCase
{
    [Collection("LogCall collection")]
    public class CreateContactUseCaseTests : LogCallAspectFixture
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly Mock<ISnsGateway> _mockSnsGateway;
        private readonly Mock<ISnsFactory> _mockSnsFactory;
        private readonly CreateContactUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public CreateContactUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _mockSnsGateway = new Mock<ISnsGateway>();
            _mockSnsFactory = new Mock<ISnsFactory>();

            _classUnderTest = new CreateContactUseCase(_mockGateway.Object, _mockSnsGateway.Object, _mockSnsFactory.Object);
        }

        [Fact]
        public async Task CreateContactReturnsContact()
        {
            // Arrange
            var contact = _fixture.Create<ContactDetails>();
            var token = _fixture.Create<Token>();
            var request = _fixture.Create<ContactDetailsRequestObject>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>()))
                .ReturnsAsync(contact);

            // Act
            var response = await _classUnderTest.ExecuteAsync(request, token).ConfigureAwait(false);

            // Assert
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Fact]
        public async Task CreateContactPublishes()
        {
            // Arrange
            var contact = _fixture.Create<ContactDetails>();
            var token = _fixture.Create<Token>();
            var request = _fixture.Create<ContactDetailsRequestObject>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>()))
                .ReturnsAsync(contact);

            // Act
            _ = await _classUnderTest.ExecuteAsync(request, token).ConfigureAwait(false);

            // Assert
            _mockSnsFactory.Verify(x => x.Create(It.IsAny<ContactDetails>(), It.IsAny<Token>(), It.IsAny<string>()));
            _mockSnsGateway.Verify(x => x.Publish(It.IsAny<ContactDetailsSns>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void CreateContactThrowsException()
        {
            // Arrange
            var exception = new ApplicationException("Test Exception");
            var token = _fixture.Create<Token>();
            var request = _fixture.Create<ContactDetailsRequestObject>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>())).ThrowsAsync(exception);

            // Act
            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.ExecuteAsync(request, token).ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
