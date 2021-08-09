using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class AddressExtendedValidatorTests
    {
        private readonly AddressExtendedValidator _sut;
        private const string StringWithTags = "Some string with <tag> in it.";

        public AddressExtendedValidatorTests()
        {
            _sut = new AddressExtendedValidator();
        }

        [Fact]
        public void ShouldErrorWithTagsInUPRN()
        {
            var model = new AddressExtended() { UPRN = StringWithTags };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UPRN)
                  .WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("some-valid-uprn")]
        public void ShouldNotErrorValidUPRN(string uprn)
        {
            var model = new AddressExtended() { UPRN = uprn };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.UPRN);
        }

        [Fact]
        public void ShouldErrorWithTagsInOverseasAddress()
        {
            var model = new AddressExtended() { OverseasAddress = StringWithTags };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.OverseasAddress)
                  .WithErrorCode(ErrorCodes.XssCheckFailure);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("some-valid-uprn")]
        public void ShouldNotErrorValidOverseasAddress(string address)
        {
            var model = new AddressExtended() { OverseasAddress = address };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.OverseasAddress);
        }
    }
}
