using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;
using AddressExtended = ContactDetailsApi.V2.Domain.AddressExtended;

namespace ContactDetailsApi.V2.Boundary.Request.Validation
{
    public class AddressExtendedValidator : AbstractValidator<AddressExtended>
    {
        private const string PostCodeRegEx = @"^((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]))))( )?(([0-9][A-Za-z]?[A-Za-z]?)?))$";

        public AddressExtendedValidator(ContactType contactType)
        {
            RuleFor(x => x.UPRN).NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.UPRN));

            RuleFor(x => x.OverseasAddress).NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.OverseasAddress));

            RuleFor(x => x.AddressLine1)
                .NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.AddressLine1));

            RuleFor(x => x.AddressLine2)
                .NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.AddressLine2));

            RuleFor(x => x.AddressLine3)
                .NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.AddressLine3));

            RuleFor(x => x.AddressLine4)
                .NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.AddressLine4));

            RuleFor(x => x.PostCode)
                .NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.PostCode));

            // PostCode and AddressLine1 are only required when the ContactType is 'address'
            When(x => contactType == ContactType.address, () =>
            {
                RuleFor(x => x.AddressLine1)
                    .NotNull()
                    .NotEmpty();

                RuleFor(x => x.PostCode)
                    .NotNull()
                    .NotEmpty();

                RuleFor(x => x.PostCode).Matches(PostCodeRegEx)
                    .WithErrorCode(ErrorCodes.InvalidEmail);
            });
        }
    }
}
