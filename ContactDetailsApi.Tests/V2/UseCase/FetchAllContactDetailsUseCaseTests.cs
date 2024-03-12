using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using ContactDetailsApi.V2.Boundary.Request;

namespace ContactDetailsApi.Tests.V2.UseCase
{
    [Collection("LogCall collection")]
    public class FetchAllContactDetailsUseCaseTests
    {
        private readonly FetchAllContactDetailsByUprnUseCase _classUnderTest;
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly Fixture _fixture = new Fixture();

        public FetchAllContactDetailsUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new FetchAllContactDetailsByUprnUseCase(_mockGateway.Object);

        }

        [Fact]
        public async Task FetchAllContactDetailsShouldBeNull()
        {
            // Arrange
            var query = _fixture.Create<FetchAllContactDetailsQuery>();
            _mockGateway
                .Setup(x => x.FetchAllContactDetailsByUprnUseCase(query))
                .ReturnsAsync((List<ContactByUprn>) null);

            // Act
            var response = await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public async Task FetchAllContactDetailsReturnsOkResponse()
        {
            // Arrange
            var query = _fixture.Create<FetchAllContactDetailsQuery>();
            var allContacts = _fixture.Create<List<ContactByUprn>>();

            _mockGateway
                .Setup(x => x.FetchAllContactDetailsByUprnUseCase(query))
                .ReturnsAsync(allContacts);

            // Act
            var response = await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            response.Should().BeEquivalentTo(allContacts);
        }

        [Fact]
        public void FetchAllContactDetailsThrowsException()
        {
            // Arrange
            var query = _fixture.Create<FetchAllContactDetailsQuery>();
            var exception = new ApplicationException("Test Exception");

            _mockGateway
                .Setup(x => x.FetchAllContactDetailsByUprnUseCase(query))
                .ThrowsAsync(exception);

            // Act
            Func<Task<List<ContactByUprn>>> func = async () => await _classUnderTest.ExecuteAsync(query).ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
