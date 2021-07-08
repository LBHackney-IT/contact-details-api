using System;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Infrastructure;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.Factories
{
    public class ContactDetailsSnsFactory : ISnsFactory
    {
        public ContactDetailsSns Create(ContactDetailsRequestObject contactDetails, Token token)
        {
            return new ContactDetailsSns
            {
                CorrelationId = Guid.NewGuid().ToString(),
                DateTime = DateTime.UtcNow,
                EntityId = contactDetails.Id,
                Id = Guid.NewGuid(),
                EventType = CreateEventConstants.EVENTTYPE,
                Version = CreateEventConstants.V1VERSION,
                SourceDomain = CreateEventConstants.SOURCEDOMAIN,
                SourceSystem = CreateEventConstants.SOURCESYSTEM,
                User = new Domain.Sns.User { Id = Guid.NewGuid(), Name = token.Name, Email = token.Email }
            };
        }
    }
}
