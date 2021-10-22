using ContactDetailsApi.V2.Domain;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using ContactType = ContactDetailsApi.V1.Domain.ContactType;

namespace ContactDetailsApi.V2.Boundary.Request.Validation
{
    public class CanCreateContactValidator : AbstractValidator<CanCreateContactRequest>
    {
        private const int MAX_EMAIL_CONTACTS = 5;
        private const int MAX_PHONE_CONTACTS = 5;

        public CanCreateContactValidator()
        {
            RuleFor(x => x.ExistingContacts)
                .Must(x => x.Count(x => x.ContactInformation.ContactType == ContactType.email) < MAX_EMAIL_CONTACTS)
                .WithMessage(x => FormatErrorMessage(ContactType.email, MAX_EMAIL_CONTACTS, x.Request.TargetId))
                .When(x => x.Request.ContactInformation.ContactType == ContactType.email);

            RuleFor(x => x.ExistingContacts)
                .Must(x => x.Count(x => x.ContactInformation.ContactType == ContactType.phone) < MAX_PHONE_CONTACTS)
                .WithMessage(x => FormatErrorMessage(ContactType.phone, MAX_PHONE_CONTACTS, x.Request.TargetId))
                .When(x => x.Request.ContactInformation.ContactType == ContactType.phone);
        }

        private static string FormatErrorMessage(ContactType type, int maxAllowed, Guid targetId)
        {
            var typeString = Enum.GetName(typeof(ContactType), type);
            return $"Cannot create {typeString} contact record for targetId {targetId} as the maximum for that type ({maxAllowed}) has already been reached.";
        }
    }

    public class CanCreateContactRequest
    {
        public ContactDetailsRequestObject Request { get; set; }
        public List<ContactDetails> ExistingContacts { get; set; } = new List<ContactDetails>();
    }
}
