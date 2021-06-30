using FluentValidation;
using System;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class ContactDetailsRequestObjectValidator : AbstractValidator<ContactDetailsRequestObject>
    {
        public ContactDetailsRequestObjectValidator()
        {
            RuleFor(x => x.TargetId).NotNull()
                                    .NotEqual(Guid.Empty);
            RuleFor(x => x.TargetType).IsInEnum();
            RuleFor(x => x.ContactInformation).NotNull()
                                              .SetValidator(new ContactInformationValidator());
            RuleFor(x => x.SourceServiceArea).NotNull()
                                             .SetValidator(new SourceServiceAreaValidator());
            RuleFor(x => x.RecordValidUntil).NotEqual(default(DateTime))
                                            .When(x => x.RecordValidUntil.HasValue);
        }
    }
}
