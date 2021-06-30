using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class AddressExtendedValidatorTests
    {
        private readonly AddressExtendedValidator _sut;

        public AddressExtendedValidatorTests()
        {
            _sut = new AddressExtendedValidator();
        }

        [Fact]
        public void ShouldErrorWithInvalidUPRN()
        {
            var model = new AddressExtended() { UPRN = "Some<tag>value" };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.UPRN);
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
        public void ShouldErrorWithInvalidOverseasAddress()
        {
            var model = new AddressExtended() { OverseasAddress = "Some<tag>value" };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.OverseasAddress);
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
