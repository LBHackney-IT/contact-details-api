using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ContactDetailsRequestObject = ContactDetailsApi.V2.Boundary.Request.ContactDetailsRequestObject;

namespace ContactDetailsApi.Tests.V2.Controller
{
    [Collection("LogCall collection")]
    public class ContactDetailsControllerTests
    {
        private readonly ContactDetailsController _classUnderTest;
        private readonly Mock<ICreateContactUseCase> _mockCreateContactUseCase;
        private readonly Mock<IGetContactDetailsByTargetIdUseCase> _mockGetByIdUseCase;
        private readonly Mock<IHttpContextWrapper> _mockHttpContextWrapper;
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Fixture _fixture = new Fixture();

        public ContactDetailsControllerTests()
        {
            _mockCreateContactUseCase = new Mock<ICreateContactUseCase>();
            _mockGetByIdUseCase = new Mock<IGetContactDetailsByTargetIdUseCase>();
            _mockHttpContextWrapper = new Mock<IHttpContextWrapper>();
            _mockTokenFactory = new Mock<ITokenFactory>();

            _classUnderTest = new ContactDetailsController(
                _mockCreateContactUseCase.Object,
                _mockGetByIdUseCase.Object,
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

        [Fact]
        public async Task GetContactDetailsByTargetIdNotFoundReturnsNotFound()
        {
            // Arrange
            var query = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };

            _mockGetByIdUseCase
                .Setup(x => x.Execute(query))
                .ReturnsAsync((List<ContactDetailsResponseObject>) null);

            // Act
            var response = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(query.TargetId);
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdReturnsOkResponse()
        {
            // Arrange
            var queryParam = new ContactQueryParameter { TargetId = Guid.NewGuid() };
            var contactResponse = _fixture.Create<List<ContactDetailsResponseObject>>();

            _mockGetByIdUseCase
                .Setup(x => x.Execute(queryParam))
                .ReturnsAsync((contactResponse));

            // Act
            var response = await _classUnderTest.GetContactDetailsByTargetId(queryParam).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().BeEquivalentTo(new GetContactDetailsResponse(contactResponse));
        }

        [Fact]
        public void GetContactDetailsByTargetIdThrowsException()
        {
            // Arrange
            var targetId = Guid.NewGuid();
            var queryParam = new ContactQueryParameter { TargetId = targetId };

            var exception = new ApplicationException("Test Exception");

            _mockGetByIdUseCase
                .Setup(x => x.Execute(queryParam))
                .ThrowsAsync(exception);

            // Act
            Func<Task<IActionResult>> func = async () => await _classUnderTest.GetContactDetailsByTargetId(queryParam).ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
