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
using Xunit;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class GetContactDetailsByTargetIdUseCaseTests : LogCallAspectFixture
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly GetContactDetailsByTargetIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public GetContactDetailsByTargetIdUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new GetContactDetailsByTargetIdUseCase(_mockGateway.Object);
        }

        [Fact]
        public async Task GetByIdUseCaseShouldBeEmpty()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            _mockGateway.Setup(x => x.GetContactDetailsByTargetId(queryParam)).ReturnsAsync((List<ContactDetails>) null);

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
            response.Should().BeEmpty();
        }

        [Fact]
        public async Task GetContactByIdReturnsOkResponse()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var contact = _fixture.Create<List<ContactDetails>>();
            _mockGateway.Setup(x => x.GetContactDetailsByTargetId(queryParam)).ReturnsAsync(contact);

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Fact]
        public void GetContactByIdThrowsException()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var exception = new ApplicationException("Test Exception");
            _mockGateway.Setup(x => x.GetContactDetailsByTargetId(queryParam)).ThrowsAsync(exception);

            Func<Task<List<ContactDetailsResponseObject>>> func = async () => await _classUnderTest.Execute(queryParam).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
