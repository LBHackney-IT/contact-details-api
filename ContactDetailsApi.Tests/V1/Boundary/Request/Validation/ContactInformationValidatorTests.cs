using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class ContactInformationValidatorTests
    {
        private readonly ContactInformationValidator _sut;
        private const string StringWithTags = "Some string with <tag> in it.";

        public ContactInformationValidatorTests()
        {
            _sut = new ContactInformationValidator();
        }

        private static IEnumerable<object[]> GetEnumValues<T>() where T : Enum
        {
            foreach (var val in Enum.GetValues(typeof(T)))
            {
                yield return new object[] { val };
            }
        }

        public static IEnumerable<object[]> ContactTypes => GetEnumValues<ContactType>();
        public static IEnumerable<object[]> SubTypes => GetEnumValues<SubType>();

        [Theory]
        [MemberData(nameof(ContactTypes))]
        public void ContactTypeShouldNotErrorWithValidValue(ContactType valid)
        {
            var model = new ContactInformation() { ContactType = valid };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.ContactType);
        }

        [Theory]
        [InlineData(100)]
        public void ContactTypeShouldErrorWithInvalidValue(int? val)
        {
            var model = new ContactInformation() { ContactType = (ContactType) val };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ContactType);
        }

        [Theory]
        [MemberData(nameof(SubTypes))]
        [InlineData(null)]
        public void SubTypeShouldNotErrorWithValidValue(SubType? valid)
        {
            var model = new ContactInformation() { SubType = valid };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.SubType);
        }

        [Theory]
        [InlineData(100)]
        public void SubTypeShouldErrorWithInvalidValue(int? val)
        {
            var model = new ContactInformation() { SubType = (SubType) val };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SubType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ValueShouldErrorWithNoValue(string invalid)
        {
            var model = new ContactInformation() { Value = invalid };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Value);
        }

        [Fact]
        public void ValueShouldErrorWithTagsInValue()
        {
            var model = new ContactInformation() { Value = StringWithTags };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Value)
                  .WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData("invalidEmail")]
        [InlineData("@invalidEmail")]
        [InlineData("invalidEmail@")]
        public void ValueShouldErrorWithInvalidEmail(string invalid)
        {
            var model = new ContactInformation()
            {
                Value = invalid,
                ContactType = ContactType.email
            };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Value)
                  .WithErrorCode(ErrorCodes.InvalidEmail);
        }

        [Fact]
        public void ValueShouldNotErrorWithValidEmail()
        {
            var model = new ContactInformation()
            {
                Value = "a.b@c.com",
                ContactType = ContactType.email
            };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Value);
        }

        [Theory]
        [InlineData("invalidphone")]
        [InlineData("3214")]
        public void ValueShouldErrorWithInvalidPhoneNumber(string invalid)
        {
            var model = new ContactInformation()
            {
                Value = invalid,
                ContactType = ContactType.phone
            };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Value)
                  .WithErrorCode(ErrorCodes.InvalidPhoneNumber);
        }

        [Theory]
        [InlineData("01234 654987")]
        [InlineData("(01234) 654987")]
        [InlineData("+44 1234 654987")]
        public void ValueShouldNotErrorWithValidPhoneNumber(string valid)
        {
            var model = new ContactInformation()
            {
                Value = valid,
                ContactType = ContactType.phone
            };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Value);
        }

        [Fact]
        public void DescriptionShouldErrorWithWithTagsInValue()
        {
            var model = new ContactInformation() { Description = StringWithTags };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                  .WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Mr Joe Bloggs")]
        public void DescriptionShouldNotErrorValidValue(string valid)
        {
            var model = new ContactInformation() { Description = valid };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void AddressExtendedShouldErrorWithInvalidValue()
        {
            var invalidAddressExtended = new AddressExtended() { UPRN = StringWithTags };
            var model = new ContactInformation() { AddressExtended = invalidAddressExtended };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.AddressExtended.UPRN)
                  .WithErrorCode(ErrorCodes.XssCheckFailure);
        }
    }
}
