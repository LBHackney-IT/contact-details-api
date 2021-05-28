using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class ContactQueryParameterValidatorTests
    {
        private readonly ContactQueryParameterValidator _sut;

        public ContactQueryParameterValidatorTests()
        {
            _sut = new ContactQueryParameterValidator();
        }

        [Fact]
        public void QueryShouldErrorWithNullTargetId()
        {
            var query = new ContactQueryParameter();
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.TargetId);
        }

        [Fact]
        public void QueryShouldErrorWithEmptyTargetId()
        {
            var query = new ContactQueryParameter() { TargetId = Guid.Empty };
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.TargetId);
        }
    }
}
