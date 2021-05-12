using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Controllers
{
    [Collection("LogCall collection")]
    public class ContactDetailsControllerTests
    {
        private readonly ContactDetailsController _classUnderTest;
        private readonly Mock<IGetContactDetailsByTargetIdUseCase> _mockGetByIdUseCase;
        private readonly Fixture _fixture = new Fixture();

        public ContactDetailsControllerTests()
        {
            _mockGetByIdUseCase = new Mock<IGetContactDetailsByTargetIdUseCase>();
            _classUnderTest = new ContactDetailsController(_mockGetByIdUseCase.Object);
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdNotFoundReturnsNotFound()
        {
            var cqp = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            _mockGetByIdUseCase.Setup(x => x.Execute(cqp)).ReturnsAsync((List<ContactDetailsResponseObject>) null);

            var response = await _classUnderTest.GetContactDetailsByTargetId(cqp).ConfigureAwait(false);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(cqp.TargetId);
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdReturnsOkResponse()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var contactResponse = _fixture.Create<List<ContactDetailsResponseObject>>();
            _mockGetByIdUseCase.Setup(x => x.Execute(queryParam)).ReturnsAsync((contactResponse));

            var response = await _classUnderTest.GetContactDetailsByTargetId(queryParam).ConfigureAwait(false);
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().BeEquivalentTo(new GetContactDetailsResponse(contactResponse));
        }

        [Fact]
        public void GetContactDetailsByTargetIdThrowsException()
        {
            var targetId = Guid.NewGuid();
            var queryParam = new ContactQueryParameter
            {
                TargetId = targetId
            };
            var exception = new ApplicationException("Test Exception");
            _mockGetByIdUseCase.Setup(x => x.Execute(queryParam)).ThrowsAsync(exception);

            Func<Task<IActionResult>> func = async () => await _classUnderTest.GetContactDetailsByTargetId(queryParam).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
