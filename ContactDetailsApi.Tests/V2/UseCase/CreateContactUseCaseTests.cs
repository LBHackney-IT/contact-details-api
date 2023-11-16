using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Factories.Interfaces;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase;
using FluentAssertions;
using FluentValidation;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ContactDetailsRequestObject = ContactDetailsApi.V2.Boundary.Request.ContactDetailsRequestObject;
using ContactType = ContactDetailsApi.V1.Domain.ContactType;

namespace ContactDetailsApi.Tests.V2.UseCase
{
    [Collection("LogCall collection")]
    public class CreateContactUseCaseTests
    {
        private readonly Mock<IContactDetailsGateway> _mockGateway;
        private readonly Mock<ISnsGateway> _mockSnsGateway;
        private readonly Mock<ISnsFactory> _mockSnsFactory;
        private readonly CreateContactUseCase _classUnderTest;
        private readonly Token _token;
        private readonly Fixture _fixture = new Fixture();
        private readonly Guid _targetId = Guid.NewGuid();

        private const int MAX_EMAIL_CONTACTS = 5;
        private const int MAX_PHONE_CONTACTS = 5;

        public CreateContactUseCaseTests()
        {
            _mockGateway = new Mock<IContactDetailsGateway>();
            _mockSnsGateway = new Mock<ISnsGateway>();
            _mockSnsFactory = new Mock<ISnsFactory>();

            _token = _fixture.Create<Token>();
            _classUnderTest = new CreateContactUseCase(_mockGateway.Object, _mockSnsGateway.Object, _mockSnsFactory.Object);

            _mockGateway.Setup(x => x.GetContactDetailsByTargetId(It.IsAny<ContactQueryParameter>()))
                        .ReturnsAsync(new List<ContactDetails>());
        }

        private ContactInformation CreateContactInformation(ContactType newType)
        {
            return _fixture.Build<ContactInformation>()
                           .With(x => x.ContactType, newType)
                           .Create();
        }

        private List<ContactDetails> CreateExistingContacts()
        {
            return _fixture.Build<ContactDetails>()
                           .With(x => x.TargetId, _targetId)
                           .CreateMany(3).ToList();
        }

        private List<ContactDetails> CreateTooManyExistingContacts()
        {
            var existingContacts = CreateExistingContacts();

            existingContacts.AddRange(_fixture.Build<ContactDetails>()
                                                .With(x => x.TargetId, _targetId)
                                                .With(x => x.IsActive, true)
                                                .With(x => x.ContactInformation, CreateContactInformation(ContactType.email))
                                                .CreateMany(MAX_EMAIL_CONTACTS));
            existingContacts.AddRange(_fixture.Build<ContactDetails>()
                                                .With(x => x.TargetId, _targetId)
                                                .With(x => x.IsActive, true)
                                                .With(x => x.ContactInformation, CreateContactInformation(ContactType.phone))
                                                .CreateMany(MAX_PHONE_CONTACTS));

            return existingContacts;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateContactReturnsContact(bool hasExisting)
        {
            // Arrange
            var contact = _fixture.Create<ContactDetails>();
            var request = _fixture.Create<ContactDetailsRequestObject>();
            if (hasExisting)
                _mockGateway.Setup(x => x.GetContactDetailsByTargetId(It.IsAny<ContactQueryParameter>()))
                            .ReturnsAsync(CreateExistingContacts());

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>()))
                .ReturnsAsync(contact);

            // Act
            var response = await _classUnderTest.ExecuteAsync(request, _token).ConfigureAwait(false);

            // Assert
            response.Should().BeEquivalentTo(contact.ToResponse());
        }

        [Theory]
        [InlineData(ContactType.email)]
        [InlineData(ContactType.phone)]
        public void CreateContactTooManyExistingThrows(ContactType newType)
        {
            // Arrange
            var contactInfo = CreateContactInformation(newType);
            var request = _fixture.Build<ContactDetailsRequestObject>()
                                  .With(x => x.TargetId, _targetId)
                                  .With(x => x.ContactInformation, contactInfo)
                                  .Create();

            _mockGateway.Setup(x => x.GetContactDetailsByTargetId(It.IsAny<ContactQueryParameter>()))
                        .ReturnsAsync(CreateTooManyExistingContacts());

            // Act
            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.ExecuteAsync(request, _token).ConfigureAwait(false);

            // Assert
            var typeString = Enum.GetName(typeof(ContactType), newType);
            func.Should().Throw<ValidationException>().WithMessage($"*Cannot create {typeString} contact record for targetId {_targetId} as the " +
                                    "maximum for that type (5) has already been reached*");
        }

        [Fact]
        public async Task CreateContactPublishes()
        {
            // Arrange
            var contact = _fixture.Create<ContactDetails>();
            var request = _fixture.Create<ContactDetailsRequestObject>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>()))
                .ReturnsAsync(contact);

            // Act
            _ = await _classUnderTest.ExecuteAsync(request, _token).ConfigureAwait(false);

            // Assert
            _mockSnsFactory.Verify(x => x.Create(It.IsAny<ContactDetails>(), _token, It.IsAny<string>()));
            _mockSnsGateway.Verify(x => x.Publish(It.IsAny<ContactDetailsSns>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public void CreateContactThrowsException()
        {
            // Arrange
            var exception = new ApplicationException("Test Exception");
            var request = _fixture.Create<ContactDetailsRequestObject>();

            _mockGateway.Setup(x => x.CreateContact(It.IsAny<ContactDetailsEntity>())).ThrowsAsync(exception);

            // Act
            Func<Task<ContactDetailsResponseObject>> func = async () => await _classUnderTest.ExecuteAsync(request, _token).ConfigureAwait(false);

            // Assert
            func.Should().Throw<ApplicationException>().WithMessage(exception.Message);
        }
    }
}
