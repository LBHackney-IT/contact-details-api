using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;
using System;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class CreatedByValidator : AbstractValidator<CreatedBy>
    {
        public CreatedByValidator()
        {
            RuleFor(x => x.Id).NotNull()
                              .NotEqual(Guid.Empty);
            RuleFor(x => x.CreatedAt).NotNull()
                                     .NotEqual(default(DateTime))
                                     .LessThan(DateTime.UtcNow);
            RuleFor(x => x.FullName).NotXssString()
                                    .When(x => !string.IsNullOrWhiteSpace(x.FullName));
            RuleFor(x => x.EmailAddress).EmailAddress()
                                        .When(x => !string.IsNullOrWhiteSpace(x.EmailAddress));
        }
    }
}
