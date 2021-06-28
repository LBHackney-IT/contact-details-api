using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class CreatedByValidatorTests
    {
        private readonly CreatedByValidator _sut;

        public CreatedByValidatorTests()
        {
            _sut = new CreatedByValidator();
        }

        [Fact]
        public void ShouldErrorWithEmptyId()
        {
            var model = new CreatedBy() { Id = Guid.Empty };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void CreatedAtShouldErrorWithEmptyValue()
        {
            var model = new CreatedBy() { CreatedAt = default };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CreatedAt);
        }

        [Fact]
        public void CreatedAtShouldErrorWithFutureValue()
        {
            var model = new CreatedBy() { CreatedAt = DateTime.UtcNow.AddDays(1) };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CreatedAt);
        }

        [Fact]
        public void ShouldErrorWithInvalidFullName()
        {
            var model = new CreatedBy() { FullName = "Some<tag>value" };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.FullName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Mr Joe Bloggs")]
        public void ShouldNotErrorValidFullName(string fullName)
        {
            var model = new CreatedBy() { FullName = fullName };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        }

        [Fact]
        public void ShouldErrorWithInvalidEmailAddress()
        {
            var model = new CreatedBy() { EmailAddress = "sdfsdkfjsdf" };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a.b@c.com")]
        public void ShouldNotErrorValidEmailAddress(string email)
        {
            var model = new CreatedBy() { EmailAddress = email };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.EmailAddress);
        }
    }
}
