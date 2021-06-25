using ContactDetailsApi.V1.Domain;
using FluentValidation;
using Hackney.Core.Validation;

namespace ContactDetailsApi.V1.Boundary.Request.Validation
{
    public class ContactInformationValidator : AbstractValidator<ContactInformation>
    {
        // TODO - Create a common validator for this
        private const string UkPhoneNumberRegEx
            = @"^(((\+44\s?\d{4}|\(?0\d{4}\)?)\s?\d{3}\s?\d{3})|((\+44\s?\d{3}|\(?0\d{3}\)?)\s?\d{3}\s?\d{4})|((\+44\s?\d{2}|\(?0\d{2}\)?)\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$";

        public ContactInformationValidator()
        {
            RuleFor(x => x.ContactType).IsInEnum();
            RuleFor(x => x.SubType).IsInEnum()
                                   .When(x => x.SubType.HasValue);
            RuleFor(x => x.Value).NotNull()
                                 .NotEmpty()
                                 .NotXssString();
            RuleFor(x => x.Value).EmailAddress()
                                 .When(x => x.ContactType == ContactType.email);
            RuleFor(x => x.Value).Matches(UkPhoneNumberRegEx)
                                 .When(y => y.ContactType == ContactType.phone);
            RuleFor(x => x.Description).NotXssString()
                                       .When(x => !string.IsNullOrWhiteSpace(x.Description));
            RuleFor(x => x.AddressExtended).SetValidator(new AddressExtendedValidator());
        }
    }
}
