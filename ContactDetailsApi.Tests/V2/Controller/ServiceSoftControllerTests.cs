using AutoFixture;
using ContactDetailsApi.V2.Controllers;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Response;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Controller
{
    [Collection("LogCall collection")]
    public class ServiceSoftControllerTests
    {
        private readonly ServicesoftController _classUnderTest;
        private readonly Mock<IFetchAllContactDetailsByUprnUseCase> _mockFetchAllContactDetailsByUprn;
        private readonly Fixture _fixture = new Fixture();

        public ServiceSoftControllerTests()
        {
            _mockFetchAllContactDetailsByUprn = new Mock<IFetchAllContactDetailsByUprnUseCase>();
            _classUnderTest = new ServicesoftController(_mockFetchAllContactDetailsByUprn.Object);
        }

        [Fact]
        public async Task FetchAllContactDetailsReturns200Response()
        {
            // Arrange
            var response = _fixture.Create<ContactsByUprnList>();

            _mockFetchAllContactDetailsByUprn.Setup(x => x.ExecuteAsync()).ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.FetchAllContactDetailsByUprn().ConfigureAwait(false);

            // Assert
            result.Should().BeOfType(typeof(OkObjectResult));
            (result as OkObjectResult).Value.Should().BeEquivalentTo(response);
        }

        [Fact]
        public void FetchAllContactDetailsThrowsException()
        {
            // Arrange
            var exception = new ApplicationException("Test Exception");

            _mockFetchAllContactDetailsByUprn.Setup(x => x.ExecuteAsync()).ThrowsAsync(exception);

            // Act
            Func<Task<IActionResult>> func = async () => await _classUnderTest.FetchAllContactDetailsByUprn().ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }

    }
}
