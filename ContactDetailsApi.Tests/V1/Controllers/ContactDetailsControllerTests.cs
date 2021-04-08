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
        private Mock<IGetByIdUseCase> _mockGetByIdUseCase;
        private readonly Fixture _fixture = new Fixture();


        [SetUp]
        public void SetUp()
        {
            _mockGetByIdUseCase = new Mock<IGetByIdUseCase>();
            _classUnderTest = new ContactDetailsController(_mockGetByIdUseCase.Object);
        }

        [Test]
        public async Task GetContactByIdNotFoundReturnsNotFound()
        {
            var id = Guid.NewGuid();
            var cqp = new ContactQueryParameter
            {
                TargetId = id
            };
            _mockGetByIdUseCase.Setup(x => x.Execute(cqp)).ReturnsAsync((ContactDetailsResponseObject) null);

            var response = await _classUnderTest.GetContactByTargetId(cqp).ConfigureAwait(false);
            response.Should().BeOfType(typeof(NotFoundObjectResult));
            (response as NotFoundObjectResult).Value.Should().Be(cqp);
        }

        [Test]
        public async Task GetContactbyIdReturnsOkResponse()
        {
            var id = Guid.NewGuid();
            var cqp = new ContactQueryParameter
            {
                TargetId = id
            };
            var contactResponse = _fixture.Create<ContactDetailsResponseObject>();
            _mockGetByIdUseCase.Setup(x => x.Execute(cqp)).ReturnsAsync((contactResponse));

            var response = await _classUnderTest.GetContactByTargetId(cqp).ConfigureAwait(false);
            response.Should().BeOfType(typeof(OkObjectResult));
            (response as OkObjectResult).Value.Should().Be(contactResponse);
        }
        [Test]
        public void GetContactByIdThrowsException()
        {
            var id = Guid.NewGuid();
            var cqp = new ContactQueryParameter
            {
                TargetId = id
            };
            var exception = new ApplicationException("Test Exception");
            _mockGetByIdUseCase.Setup(x => x.Execute(cqp)).ThrowsAsync(exception);

            Func<Task<IActionResult>> func = async () => await _classUnderTest.GetContactByTargetId(cqp).ConfigureAwait(false);

            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
