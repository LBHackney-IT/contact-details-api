using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class AddressExtendedValidator : AbstractValidator<AddressExtended>
    {
        public AddressExtendedValidator()
        {
            RuleFor(x => x.UPRN).NotXssString()
                                .WithErrorCode(ErrorCodes.XssCheckFailure)
                                .When(x => !string.IsNullOrWhiteSpace(x.UPRN));
            RuleFor(x => x.OverseasAddress).NotXssString()
                                           .WithErrorCode(ErrorCodes.XssCheckFailure)
                                           .When(x => !string.IsNullOrWhiteSpace(x.OverseasAddress));
        }
    }
}
