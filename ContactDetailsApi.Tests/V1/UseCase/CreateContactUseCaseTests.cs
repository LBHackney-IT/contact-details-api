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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hackney.Core.JWT;
using Xunit;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class CreateContactUseCaseTests : LogCallAspectFixture
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly CreateContactUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public CreateContactUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new CreateContactUseCase(_mockGateway.Object);
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
