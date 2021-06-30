using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Request.Validation;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Boundary.Request.Validation
{
    public class DeleteContactQueryParameterValidatorTests
    {
        private readonly DeleteContactQueryParameterValidator _sut;

        public DeleteContactQueryParameterValidatorTests()
        {
            _sut = new DeleteContactQueryParameterValidator();
        }

        [Fact]
        public void QueryShouldErrorWithNullTargetId()
        {
            var query = new DeleteContactQueryParameter();
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.TargetId);
        }

        [Fact]
        public void QueryShouldErrorWithEmptyTargetId()
        {
            var query = new DeleteContactQueryParameter() { TargetId = Guid.Empty };
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.TargetId);
        }
        [Fact]
        public void QueryShouldErrorWithNullId()
        {
            var query = new DeleteContactQueryParameter();
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public void QueryShouldErrorWithEmptyId()
        {
            var query = new DeleteContactQueryParameter() { Id = Guid.Empty };
            var result = _sut.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
