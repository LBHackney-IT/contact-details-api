using FluentValidation;
using System;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class DeleteContactQueryParameterValidator : AbstractValidator<DeleteContactQueryParameter>
    {
        public DeleteContactQueryParameterValidator()
        {
            RuleFor(x => x.TargetId).NotNull()
                                    .NotEqual(Guid.Empty);

            RuleFor(x => x.Id).NotNull()
                                    .NotEqual(Guid.Empty);

        }
    }
}
