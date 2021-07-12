using System;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Infrastructure;
using Hackney.Core.JWT;
using Hackney.Shared.Sns;

namespace ContactDetailsApi.V1.Factories
{
    public class ContactDetailsSnsFactory : ISnsFactory
    {
        public ContactDetailsSns Create(ContactDetails contactDetails, Token token, string eventType)
        {
            var contactDetailsSns = CreateContactDetailsSns(contactDetails, token, eventType);
            PopulateEventOldAndNewData(contactDetails, eventType, contactDetailsSns);

            return contactDetailsSns;
        }

        private static ContactDetailsSns CreateContactDetailsSns(ContactDetails contactDetails, Token token, string eventType)
        {
            return new ContactDetailsSns
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                EntityId = contactDetails.Id,
                Id = Guid.NewGuid(),
                EventType = eventType,
                Version = CreateEventConstants.V1VERSION,
                SourceDomain = CreateEventConstants.SOURCEDOMAIN,
                SourceSystem = CreateEventConstants.SOURCESYSTEM,
                User = new Domain.Sns.User { Id = Guid.NewGuid(), Name = token.Name, Email = token.Email }
            };
        }

        private static void PopulateEventOldAndNewData(ContactDetails contactDetails, string eventType,
            ContactDetailsSns contactDetailsSns)
        {
            switch (eventType)
            {
                case ContactDetailsConstants.CREATED:
                    contactDetailsSns.EventData = new EventData
                    {
                        NewData = new DataItem { Value = contactDetails.ContactInformation.Value }
                    };
                    break;
                case ContactDetailsConstants.DELETED:
                    contactDetailsSns.EventData = new EventData
                    {
                        OldData = new DataItem { Value = contactDetails.ContactInformation.Value }
                    };
                    break;
                default:
                    throw new NotImplementedException($"Event {eventType} not recognized");
            }
        }
    }
}
