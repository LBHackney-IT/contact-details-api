using FluentValidation;
using System;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class ContactQueryParameterValidator : AbstractValidator<ContactQueryParameter>
    {
        public ContactQueryParameterValidator()
        {
            RuleFor(x => x.TargetId).NotNull()
                                    .NotEqual(Guid.Empty);
        }
    }
}
