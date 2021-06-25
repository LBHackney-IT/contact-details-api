using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation.TestHelper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class ContactDetailsRequestObjectValidatorTests
    {
        private readonly ContactDetailsRequestObjectValidator _sut;

        public ContactDetailsRequestObjectValidatorTests()
        {
            _sut = new ContactDetailsRequestObjectValidator();
        }

        private static IEnumerable<object[]> GetEnumValues<T>() where T : Enum
        {
            foreach (var val in Enum.GetValues(typeof(T)))
            {
                yield return new object[] { val };
            }
        }

        public static IEnumerable<object[]> TargetTypes => GetEnumValues<TargetType>();


        [Fact]
        public void ShouldErrorWithEmptyTargetId()
        {
            var model = new ContactDetailsRequestObject() { TargetId = Guid.Empty };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.TargetId);
        }

        [Theory]
        [MemberData(nameof(TargetTypes))]
        public void TargetTypeShouldNotErrorWithValidValue(TargetType valid)
        {
            var model = new ContactDetailsRequestObject() { TargetType = valid };
            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.TargetType);
        }

        [Theory]
        [InlineData(100)]
        public void ContactTypeShouldErrorWithInvalidValue(int? val)
        {
            var model = new ContactDetailsRequestObject() { TargetType = (TargetType) val };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.TargetType);
        }

        [Fact]
        public void ContactInformationShouldErrorWhenNull()
        {
            var model = new ContactDetailsRequestObject() { ContactInformation = null };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ContactInformation);
        }

        [Fact]
        public void ContactInformationShouldErrorWithInvalidValue()
        {
            var invalid = new ContactInformation();
            var model = new ContactDetailsRequestObject() { ContactInformation = invalid };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.ContactInformation.Value);
        }

        [Fact]
        public void SourceServiceAreaShouldErrorWhenNull()
        {
            var model = new ContactDetailsRequestObject() { SourceServiceArea = null };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SourceServiceArea);
        }

        [Fact]
        public void SourceServiceAreaShouldErrorWithInvalidValue()
        {
            var invalid = new SourceServiceArea() { Area = "Some<tag>value" };
            var model = new ContactDetailsRequestObject() { SourceServiceArea = invalid };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.SourceServiceArea.Area);
        }

        [Fact]
        public void RecordValidUntilShouldErrorWithInvalidValue()
        {
            var model = new ContactDetailsRequestObject() { RecordValidUntil = default(DateTime) };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.RecordValidUntil);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("2021-05-12")]
        [InlineData("2021-05-12T12:12:12.000Z")]
        public void RecordValidUntilShouldNotErrorValidValue(string valid)
        {
            var model = new ContactDetailsRequestObject();
            if (valid != null)
                model.RecordValidUntil = DateTime.Parse(valid);

            var result = _sut.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.RecordValidUntil);
        }

        [Fact]
        public void CreatedByShouldErrorWhenNull()
        {
            var model = new ContactDetailsRequestObject() { CreatedBy = null };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CreatedBy);
        }

        [Fact]
        public void CreatedByShouldErrorWithInvalidValue()
        {
            var invalid = new CreatedBy();
            var model = new ContactDetailsRequestObject() { CreatedBy = invalid };
            var result = _sut.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.CreatedBy.Id);
        }
    }
}
