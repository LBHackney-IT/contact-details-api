using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class SourceServiceAreaValidator : AbstractValidator<SourceServiceArea>
    {
        public SourceServiceAreaValidator()
        {
            RuleFor(x => x.Area).NotXssString()
                                .WithErrorCode(ErrorCodes.XssCheckFailure)
                                .When(x => !string.IsNullOrWhiteSpace(x.Area));
        }
    }
}
