using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class SourceServiceAreaValidatorTests
    {
        private readonly SourceServiceAreaValidator _sut;

        public SourceServiceAreaValidatorTests()
        {
            _sut = new SourceServiceAreaValidator();
        }

        [Fact]
        public void ShouldErrorWithInvalidArea()
        {
            var model = new SourceServiceArea() { Area = "Some<tag>value" };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Area);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("some valid area")]
        public void ShouldNotErrorValidArea(string uprn)
        {
            var model = new SourceServiceArea() { Area = uprn };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Area);
        }
    }
}
