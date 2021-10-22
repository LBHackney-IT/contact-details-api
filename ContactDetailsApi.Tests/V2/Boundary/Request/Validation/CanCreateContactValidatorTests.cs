using AutoFixture;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request.Validation;
using ContactDetailsApi.V2.Domain;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ContactType = ContactDetailsApi.V1.Domain.ContactType;

namespace ContactDetailsApi.Tests.V2.Boundary.Request.Validation
{
    public class CanCreateContactValidatorTests
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly CanCreateContactValidator _sut;

        private const int MAX_EMAIL_CONTACTS = 5;
        private const int MAX_PHONE_CONTACTS = 5;

        public CanCreateContactValidatorTests()
        {
            _sut = new CanCreateContactValidator();
        }

        private CanCreateContactRequest CreateRequest(ContactType newType, int minExistingEmail = 0, int minExistingPhone = 0)
        {
            var contactRetailsRequest = _fixture.Build<ContactDetailsRequestObject>()
                                                .With(x => x.ContactInformation, CreateContactInformation(newType))
                                                .Create();

            var existingContacts = _fixture.Build<ContactDetails>()
                                           .With(x => x.TargetId, contactRetailsRequest.TargetId)
                                           .CreateMany(3).ToList();

            if (minExistingEmail > 0)
                existingContacts.AddRange(_fixture.Build<ContactDetails>()
                                                  .With(x => x.TargetId, contactRetailsRequest.TargetId)
                                                  .With(x => x.ContactInformation, CreateContactInformation(ContactType.email))
                                                  .CreateMany(minExistingEmail));
            if (minExistingPhone > 0)
                existingContacts.AddRange(_fixture.Build<ContactDetails>()
                                                  .With(x => x.TargetId, contactRetailsRequest.TargetId)
                                                  .With(x => x.ContactInformation, CreateContactInformation(ContactType.phone))
                                                  .CreateMany(minExistingPhone));

            return new CanCreateContactRequest()
            {
                Request = contactRetailsRequest,
                ExistingContacts = existingContacts
            };
        }

        private ContactInformation CreateContactInformation(ContactType newType)
        {
            return _fixture.Build<ContactInformation>()
                           .With(x => x.ContactType, newType)
                           .Create();
        }

        [Theory]
        [InlineData(ContactType.email)]
        [InlineData(ContactType.phone)]
        public void ContactTypeShouldNotErrorWithFerwerThanMaxContacts(ContactType type)
        {
            // Arrange
            var model = CreateRequest(type);

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ExistingContacts);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        public void EmailContactTypeShouldErrorWithTooManyContacts(int minEmailContacts)
        {
            // Arrange
            var model = CreateRequest(ContactType.email, minEmailContacts);

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            var typeString = Enum.GetName(typeof(ContactType), ContactType.email);
            result.ShouldHaveValidationErrorFor(x => x.ExistingContacts)
                  .WithErrorMessage($"Cannot create {typeString} contact record for targetId {model.Request.TargetId} as the " +
                                    $"maximum for that type ({MAX_EMAIL_CONTACTS}) has already been reached.");
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        public void PhoneContactTypeShouldErrorWithTooManyContacts(int minPhoneContacts)
        {
            // Arrange
            var model = CreateRequest(ContactType.phone, 0, minPhoneContacts);

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            var typeString = Enum.GetName(typeof(ContactType), ContactType.phone);
            result.ShouldHaveValidationErrorFor(x => x.ExistingContacts)
                  .WithErrorMessage($"Cannot create {typeString} contact record for targetId {model.Request.TargetId} as the " +
                                    $"maximum for that type ({MAX_PHONE_CONTACTS}) has already been reached.");
        }
    }
}
