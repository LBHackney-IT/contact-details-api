using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using Xunit;
using AddressExtended = ContactDetailsApi.V2.Domain.AddressExtended;
using AddressExtendedValidator = ContactDetailsApi.V2.Boundary.Request.Validation.AddressExtendedValidator;

namespace ContactDetailsApi.Tests.V2.Boundary.Request.Validation
{
    public class AddressExtendedValidatorTests
    {
        private AddressExtendedValidator _sut;
        private const string StringWithTags = "Some string with <tag> in it.";

        public AddressExtendedValidatorTests()
        {
            // Set contact type as phone by default
            _sut = new AddressExtendedValidator(ContactType.phone);
        }

        [Fact]
        public void ShouldErrorWithTagsInUPRN()
        {
            // Arrange
            var model = new AddressExtended() { UPRN = StringWithTags };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UPRN).WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("some-valid-uprn")]
        public void ShouldNotErrorValidUPRN(string uprn)
        {
            // Arrange
            var model = new AddressExtended() { UPRN = uprn };

            // Act
            var result = _sut.TestValidate(model);

            // Arrange
            result.ShouldNotHaveValidationErrorFor(x => x.UPRN);
        }

        [Fact]
        public void ShouldErrorWithTagsInOverseasAddress()
        {
            // Arrange
            var model = new AddressExtended() { OverseasAddress = StringWithTags };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OverseasAddress).WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("some-valid-uprn")]
        public void ShouldNotErrorValidOverseasAddress(string address)
        {
            // Arrange
            var model = new AddressExtended() { OverseasAddress = address };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.OverseasAddress);
        }

        [Fact]
        public void ShouldErrorWithTagsInAddressLineFields()
        {
            // Arrange
            var model = new AddressExtended()
            {
                AddressLine1 = StringWithTags,
                AddressLine2 = StringWithTags,
                AddressLine3 = StringWithTags,
                AddressLine4 = StringWithTags
            };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AddressLine1).WithErrorCode(ErrorCodes.XssCheckFailure);
            result.ShouldHaveValidationErrorFor(x => x.AddressLine2).WithErrorCode(ErrorCodes.XssCheckFailure);
            result.ShouldHaveValidationErrorFor(x => x.AddressLine3).WithErrorCode(ErrorCodes.XssCheckFailure);
            result.ShouldHaveValidationErrorFor(x => x.AddressLine4).WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Fact]
        public void ShouldErrorWithTagsInPostCode()
        {
            // Arrange
            var model = new AddressExtended() { PostCode = StringWithTags };

            // Act
            var result = _sut.TestValidate(model);

            // Assert

            result.ShouldHaveValidationErrorFor(x => x.PostCode).WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Fact]
        public void ShouldNotValidatePostCodeWhenAddressTypeIsNotAddress()
        {
            // Arrange
            var model = new AddressExtended() { PostCode = "" };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.PostCode);
        }

        [Fact]
        public void ShouldNotValidateAddressLine1WhenAddressTypeIsNotAddress()
        {
            // Arrange
            var model = new AddressExtended() { AddressLine1 = "", };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.AddressLine1);
        }

        [Theory]
        [InlineData("HD7 5UZ")]
        [InlineData("HD75UZ")]
        [InlineData("hd7 5uz")]
        [InlineData("hd75uz")]
        public void ShouldNotErrorPostCode(string postCode)
        {
            // Arrange
            _sut = new AddressExtendedValidator(ContactType.address);

            var model = new AddressExtended() { PostCode = postCode };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.PostCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void ShouldErrorAddressLine1WhenAddressTypeIsAddress(string address)
        {
            // Arrange
            _sut = new AddressExtendedValidator(ContactType.address);

            var model = new AddressExtended() { AddressLine1 = address };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AddressLine1);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("hd7   5uz")]
        [InlineData("adasd")]
        public void ShouldErrorPostCodeWhenAddressTypeIsAddress(string postCode)
        {
            // Arrange
            _sut = new AddressExtendedValidator(ContactType.address);

            var model = new AddressExtended() { PostCode = postCode };

            // Act
            var result = _sut.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PostCode);
        }
    }
}
