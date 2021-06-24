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
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [Collection("LogCall collection")]
    public class DeleteContactDetailsByTargetIdUseCaseTests : LogCallAspectFixture
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly DeleteContactDetailsByTargetIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();

        public DeleteContactDetailsByTargetIdUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new DeleteContactDetailsByTargetIdUseCase(_mockGateway.Object);
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

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
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

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
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

            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.Execute(queryParam).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
