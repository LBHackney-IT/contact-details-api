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
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.UseCase
{
    [TestFixture]
    public class GetByIdUseCaseTests
    {
        private Mock<IContactDetailsGateway> _mockGateway;
        private GetByIdUseCase _classUnderTest;
        private readonly Fixture _fixture = new Fixture();


        [SetUp]
        public void SetUp()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _classUnderTest = new GetByIdUseCase(_mockGateway.Object);
        }
        [Test]
        public async Task GetByIdUseCaseShouldBeNull()
        {
            var id = Guid.NewGuid();
            var cqp = new ContactQueryParameter
            {
                TargetId = id
            };
            _mockGateway.Setup(x => x.GetEntityById(cqp.TargetId)).ReturnsAsync((ContactDetails) null);

            var response = await _classUnderTest.Execute(cqp).ConfigureAwait(false);
            response.Should().BeNull();

        }

        [Test]
        public async Task GetContactByIdReturnsOkResponse()
        {
            var id = Guid.NewGuid();
            var cqp = new ContactQueryParameter
            {
                TargetId = id
            };
            var contact = _fixture.Create<ContactDetails>();
            _mockGateway.Setup(x => x.GetEntityById(cqp.TargetId)).ReturnsAsync(contact);

            var response = await _classUnderTest.Execute(cqp).ConfigureAwait(false);
            response.Should().BeEquivalentTo(contact.ToResponse());
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
            _mockGateway.Setup(x => x.GetEntityById(cqp.TargetId)).ThrowsAsync(exception);

            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.Execute(cqp).ConfigureAwait(false);


            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }


    }
}
