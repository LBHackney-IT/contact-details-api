using ContactDetailsApi.V1.Boundary.Request.Validation;
using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.V2.Boundary.Request.Validation
{
    public class ContactInformationValidator : AbstractValidator<ContactInformation>
    {
        // TODO - Create a common validators for these
        //private const string UkPhoneNumberRegEx
        //    = @"^(((\+44\s?\d{4}|\(?0\d{4}\)?)\s?\d{3}\s?\d{3})|((\+44\s?\d{3}|\(?0\d{3}\)?)\s?\d{3}\s?\d{4})|((\+44\s?\d{2}|\(?0\d{2}\)?)\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$";

        private const string IntPhoneNumberRegEx = @"^[+]?([0-9]*[\.\s\-\(\)]|[0-9]+){6,24}$";

        public ContactInformationValidator()
        {
            RuleFor(x => x.ContactType).IsInEnum();

            RuleFor(x => x.SubType).IsInEnum().When(x => x.SubType.HasValue);

            RuleFor(x => x.Value).NotNull().NotEmpty();

            RuleFor(x => x.Value).NotXssString()
                .WithErrorCode(ErrorCodes.XssCheckFailure)
                .When(x => !string.IsNullOrWhiteSpace(x.Value));

            RuleFor(x => x.Value).EmailAddress()
                .WithErrorCode(ErrorCodes.InvalidEmail)
                .When(x => x.ContactType == ContactType.email);

            RuleFor(x => x.Value).Matches(IntPhoneNumberRegEx)
                .WithErrorCode(ErrorCodes.InvalidPhoneNumber)
                .When(y => y.ContactType == ContactType.phone);

            RuleFor(x => x.Description).NotXssString()
                    .WithErrorCode(ErrorCodes.XssCheckFailure)
                    .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.AddressExtended).SetValidator(model => new AddressExtendedValidator(model.ContactType));
        }
    }
}
