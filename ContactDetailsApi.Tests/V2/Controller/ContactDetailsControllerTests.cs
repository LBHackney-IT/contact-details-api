using AutoFixture;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Controllers;
using ContactDetailsApi.V2.UseCase.Interfaces;
using FluentAssertions;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Controller
{
    [Collection("LogCall collection")]
    public class ContactDetailsControllerTests
    {
        private readonly ContactDetailsController _classUnderTest;
        private readonly Mock<ICreateContactUseCase> _mockCreateContactUseCase;
        private readonly Mock<IHttpContextWrapper> _mockHttpContextWrapper;
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Fixture _fixture = new Fixture();

        public ContactDetailsControllerTests()
        {
            _mockCreateContactUseCase = new Mock<ICreateContactUseCase>();
            _mockHttpContextWrapper = new Mock<IHttpContextWrapper>();
            _mockTokenFactory = new Mock<ITokenFactory>();

            _classUnderTest = new ContactDetailsController(
                _mockCreateContactUseCase.Object,
                _mockHttpContextWrapper.Object,
                _mockTokenFactory.Object);
        }

        [Fact]
        public async Task CreateContactReturnsCreatedTaskResponse()
        {
            // Arrange
            var contactRequest = _fixture.Create<ContactDetailsRequestObject>();
            var contactResponse = _fixture.Create<ContactDetailsResponseObject>();

            _mockCreateContactUseCase.Setup(x => x.ExecuteAsync(contactRequest, It.IsAny<Token>())).ReturnsAsync(contactResponse);

            // Act
            var response = await _classUnderTest.CreateContact(contactRequest).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(CreatedResult));
            (response as CreatedResult).Value.Should().BeEquivalentTo(contactResponse);
        }

        [Fact]
        public async Task CreateContactCallsTokenAndHttpWrappers()
        {
            // Arrange
            var contactRequest = _fixture.Create<ContactDetailsRequestObject>();
            var contactResponse = _fixture.Create<ContactDetailsResponseObject>();
            var token = _fixture.Create<Token>();
            _mockCreateContactUseCase.Setup(x => x.ExecuteAsync(contactRequest, token)).ReturnsAsync(contactResponse);

            // Act
            _ = await _classUnderTest.CreateContact(contactRequest).ConfigureAwait(false);

            // Assert
            _mockHttpContextWrapper.Verify(x => x.GetContextRequestHeaders(It.IsAny<HttpContext>()));
            _mockTokenFactory.Verify(x => x.Create(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()));
        }

        [Fact]
        public void CreateContactThrowsException()
        {
            // Arrange
            var contactRequest = _fixture.Create<ContactDetailsRequestObject>();
            var exception = new ApplicationException("Test Exception");

            _mockCreateContactUseCase.Setup(x => x.ExecuteAsync(contactRequest, It.IsAny<Token>())).ThrowsAsync(exception);

            // Act
            Func<Task<IActionResult>> func = async () => await _classUnderTest.CreateContact(contactRequest).ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
