using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Controllers;
using ContactDetailsApi.V1.UseCase.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.Controllers
{
    [TestFixture]
    public class ContactDetailsControllerTests
    {
        private ContactDetailsController _classUnderTest;
        private Mock<IGetContactByTargetIdUseCase> _mockGetByIdUseCase;
        private readonly Fixture _fixture = new Fixture();


        [SetUp]
        public void SetUp()
        {
            _mockGetByIdUseCase = new Mock<IGetContactByTargetIdUseCase>();
            _classUnderTest = new ContactDetailsController(_mockGetByIdUseCase.Object);
        }

        [Test]
        public async Task GetContactByIdNotFoundReturnsNotFound()
        {
            var cqp = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            _mockGetByIdUseCase.Setup(x => x.Execute(cqp)).ReturnsAsync((List<ContactDetailsResponseObject>) null);

            var response = await _classUnderTest.GetContactByTargetId(cqp).ConfigureAwait(false);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(cqp);
        }

        [Test]
        public async Task GetContactbyIdReturnsOkResponse()
        {
            var targetId = Guid.NewGuid();
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var contactResponse = _fixture.Create<List<ContactDetailsResponseObject>>();
            _mockGetByIdUseCase.Setup(x => x.Execute(queryParam)).ReturnsAsync((contactResponse));

            var response = await _classUnderTest.GetContactByTargetId(queryParam).ConfigureAwait(false);
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().Be(contactResponse);
        }
        [Test]
        public void GetContactByIdThrowsException()
        {
            var targetId = Guid.NewGuid();
            var queryParam = new ContactQueryParameter
            {
                TargetId = targetId
            };
            var exception = new ApplicationException("Test Exception");
            _mockGetByIdUseCase.Setup(x => x.Execute(queryParam)).ThrowsAsync(exception);

            Func<Task<IActionResult>> func = async () => await _classUnderTest.GetContactByTargetId(queryParam).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
