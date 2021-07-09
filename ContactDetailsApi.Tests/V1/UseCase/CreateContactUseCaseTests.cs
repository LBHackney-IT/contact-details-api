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
using ContactDetailsApi.V1.Domain.Sns;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Xunit;

namespace ContactDetailsApi.Tests.V1.UseCase
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
            var contact = _fixture.Create<ContactDetails>();
            var token = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsRequestObject>()))
                .ReturnsAsync(contact);

            var response = await _classUnderTest.ExecuteAsync(new ContactDetailsRequestObject(), token).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Fact]
        public async Task CreateContactPublishes()
        {
            var contact = _fixture.Create<ContactDetails>();
            var token = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsRequestObject>()))
                .ReturnsAsync(contact);

            var response = await _classUnderTest.ExecuteAsync(new ContactDetailsRequestObject(), token).ConfigureAwait(false);

            _mockSnsFactory.Verify(x => x.Create(It.IsAny<ContactDetails>(), It.IsAny<Token>(), It.IsAny<string>()));
            _mockSnsGateway.Verify(x => x.Publish(It.IsAny<ContactDetailsSns>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void CreateContactThrowsException()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var exception = new ApplicationException("Test Exception");
            var token = _fixture.Create<Token>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsRequestObject>())).ThrowsAsync(exception);

            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.ExecuteAsync(new ContactDetailsRequestObject(), token).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
