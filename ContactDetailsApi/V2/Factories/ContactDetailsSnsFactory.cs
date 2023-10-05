using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories.Interfaces;
using Hackney.Core.JWT;
using System;
using System.Collections.Generic;
using ContactDetails = ContactDetailsApi.V2.Domain.ContactDetails;
using EventData = ContactDetailsApi.V1.Domain.Sns.EventData;
using User = ContactDetailsApi.V1.Domain.Sns.User;

namespace ContactDetailsApi.V2.Factories
{
    public class ContactDetailsSnsFactory : ISnsFactory
    {
        private static ContactDetailsSns CreateContactDetailsSns(Guid entityId, Token token, string eventType)
        {
            return new ContactDetailsSns
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                EntityId = entityId,
                Id = Guid.NewGuid(),
                EventType = eventType,
                Version = EventConstants.V1VERSION,
                SourceDomain = EventConstants.SOURCEDOMAIN,
                SourceSystem = EventConstants.SOURCESYSTEM,
                User = new User { Name = token.Name, Email = token.Email }
            };
        }

        private static DataItem ReturnDataItem(ContactDetails contactDetails)
        {
            return new DataItem
            {
                Value = contactDetails.ContactInformation.Value,
                Id = contactDetails.Id,
                ContactType = (int) contactDetails.ContactInformation.ContactType,
                Description = contactDetails.ContactInformation.Description
            };
        }

        public ContactDetailsSns CreateEvent(ContactDetails newData, Token token)
        {
            var contactDetailsSns = CreateContactDetailsSns(newData.TargetId, token, EventConstants.CREATED);

            contactDetailsSns.EventData = new EventData
            {
                NewData = ReturnDataItem(newData),
                OldData = new DataItem()
            };

            return contactDetailsSns;
        }

        public ContactDetailsSns EditEvent(ContactDetails newData, ContactDetails oldData, Token token)
        {
            var contactDetailsSns = CreateContactDetailsSns(newData.TargetId, token, EventConstants.EDITED);

            contactDetailsSns.EventData = new EventData
            {
                NewData = ReturnDataItem(newData),
                OldData = ReturnDataItem(oldData)
            };

            return contactDetailsSns;
        }
    }
}
