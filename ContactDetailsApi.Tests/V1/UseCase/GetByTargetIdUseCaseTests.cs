using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [TestFixture]
    public class GetByTargetIdUseCaseTests
    {
        private Mock<IContactDetailsGateway> _mockGateway;
        private GetContactByTargetIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();


        [SetUp]
        public void SetUp()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new GetContactByTargetIdUseCase(_mockGateway.Object);
        }
        [Test]
        public async Task GetByIdUseCaseShouldBeEmpty()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            _mockGateway.Setup(x => x.GetContactByTargetId(queryParam.TargetId)).ReturnsAsync((List<ContactDetails>) null);

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
            response.Should().BeEmpty();

        }

        [Test]
        public async Task GetContactByIdReturnsOkResponse()
        {
            var targetId = Guid.NewGuid();
            var queryParam = new ContactQueryParameter
            {
                TargetId = targetId
            };
            var contact = _fixture.Create<List<ContactDetails>>();
            _mockGateway.Setup(x => x.GetContactByTargetId(queryParam.TargetId)).ReturnsAsync(contact);

            var response = await _classUnderTest.Execute(queryParam).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Test]
        public void GetContactByIdThrowsException()
        {
            var queryParam = new ContactQueryParameter
            {
                TargetId = Guid.NewGuid()
            };
            var exception = new ApplicationException("Test Exception");
            _mockGateway.Setup(x => x.GetContactByTargetId(queryParam.TargetId)).ThrowsAsync(exception);

            Func<Task<List<ContactDetailsResponseObject>>> func = async () => await _classUnderTest.Execute(queryParam).ConfigureAwait(false);


            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }


    }
}
