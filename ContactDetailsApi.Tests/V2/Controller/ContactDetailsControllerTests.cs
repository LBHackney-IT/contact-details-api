using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Controllers;
using ContactDetailsApi.V2.Exceptions;
using ContactDetailsApi.V2.UseCase.Interfaces;
using FluentAssertions;
using FluentValidation;
using Hackney.Core.Http;
using Hackney.Core.JWT;
using Hackney.Core.Validation.AspNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        private readonly Mock<IEditContactDetailsUseCase> _mockEditContactUseCase;
        private readonly Mock<IHttpContextWrapper> _mockHttpContextWrapper;
        private readonly Mock<ITokenFactory> _mockTokenFactory;
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly HeaderDictionary _requestHeaders;
        private readonly Mock<HttpResponse> _mockHttpResponse;
        private readonly HeaderDictionary _responseHeaders;

        private const string RequestBodyText = "Some request body text";
        private readonly MemoryStream _requestStream;

        public ContactDetailsControllerTests()
        {
            _mockCreateContactUseCase = new Mock<ICreateContactUseCase>();
            _mockGetByIdUseCase = new Mock<IGetContactDetailsByTargetIdUseCase>();
            _mockEditContactUseCase = new Mock<IEditContactDetailsUseCase>();
            _mockHttpContextWrapper = new Mock<IHttpContextWrapper>();
            _mockTokenFactory = new Mock<ITokenFactory>();

            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHttpResponse = new Mock<HttpResponse>();

            _classUnderTest = new ContactDetailsController(
                _mockCreateContactUseCase.Object,
                _mockGetByIdUseCase.Object,
                _mockHttpContextWrapper.Object,
                _mockTokenFactory.Object,
                _mockEditContactUseCase.Object);


            // changes to allow reading of raw request body
            _requestStream = new MemoryStream(Encoding.Default.GetBytes(RequestBodyText));
            _mockHttpRequest.SetupGet(x => x.Body).Returns(_requestStream);


            _requestHeaders = new HeaderDictionary();
            _mockHttpRequest.SetupGet(x => x.Headers).Returns(_requestHeaders);
            _mockHttpContextWrapper
                .Setup(x => x.GetContextRequestHeaders(It.IsAny<HttpContext>()))
            .Returns(_requestHeaders);

            _responseHeaders = new HeaderDictionary();
            _mockHttpResponse.SetupGet(x => x.Headers).Returns(_responseHeaders);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.SetupGet(x => x.Request).Returns(_mockHttpRequest.Object);
            mockHttpContext.SetupGet(x => x.Response).Returns(_mockHttpResponse.Object);

            var controllerContext = new ControllerContext(new ActionContext(mockHttpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            _classUnderTest.ControllerContext = controllerContext;
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
        public async Task CreateContactValidationExceptionReturnsBadResult()
        {
            // Arrange
            var contactRequest = _fixture.Create<ContactDetailsRequestObject>();
            var exception = new ValidationException("Test Exception");

            _mockCreateContactUseCase.Setup(x => x.ExecuteAsync(contactRequest, It.IsAny<Token>())).ThrowsAsync(exception);

            // Act
            var response = await _classUnderTest.CreateContact(contactRequest).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(BadRequestObjectResult));
            (response as BadRequestObjectResult).Value.Should().BeEquivalentTo(exception.ConstructResponse());
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

        [Fact]
        public async Task PatchContact_WhenNoneFound_ReturnsNotFoundResponse()
        {
            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            var request = _fixture.Create<EditContactDetailsRequest>();

            _mockEditContactUseCase
                .Setup(x => x.ExecuteAsync(query, It.IsAny<EditContactDetailsRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ReturnsAsync((ContactDetailsResponseObject) null);

            // Act
            var response = await _classUnderTest.PatchContact(query, request).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(NotFoundObjectResult));
        }

        [Fact]
        public async Task PatchContact_WhenConflictExceptionThrown_ReturnsConflictResponse()
        {
            // Arrange
            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            var request = _fixture.Create<EditContactDetailsRequest>();

            _mockEditContactUseCase
                .Setup(x => x.ExecuteAsync(query, It.IsAny<EditContactDetailsRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ThrowsAsync(new VersionNumberConflictException(null, null));

            // Act
            var response = await _classUnderTest.PatchContact(query, request).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(ConflictObjectResult));
        }

        [Fact]
        public async Task PatchContact_WhenUpdated_ReturnsNoContent()
        {
            // Arrange
            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            var request = _fixture.Create<EditContactDetailsRequest>();
            var useCaseResponse = new ContactDetailsResponseObject();

            _mockEditContactUseCase
                .Setup(x => x.ExecuteAsync(query, It.IsAny<EditContactDetailsRequest>(), It.IsAny<string>(), It.IsAny<Token>(), It.IsAny<int?>()))
                .ReturnsAsync(useCaseResponse);

            // Act
            var response = await _classUnderTest.PatchContact(query, request).ConfigureAwait(false);

            // Assert
            response.Should().BeOfType(typeof(NoContentResult));
        }
    }
}
