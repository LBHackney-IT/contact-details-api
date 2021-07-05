using System;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using ContactDetailsApi.V1.Boundary.Request;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.Factories
{
    public class ContactDetailsSnsFactory : ISnsFactory
    {
        public ContactDetailsSns Create(ContactDetailsRequestObject person, Token token)
        {
            return new ContactDetailsSns
            {
                CorrelationId = Guid.NewGuid(),
                DateTime = DateTime.UtcNow,
                EntityId = person.Id,
                Id = Guid.NewGuid(),
                EventType = CreateEventConstants.EVENTTYPE,
                Version = CreateEventConstants.V1VERSION,
                SourceDomain = CreateEventConstants.SOURCEDOMAIN,
                SourceSystem = CreateEventConstants.SOURCESYSTEM,
                User = new User {Id = Guid.NewGuid(), Name = token.Name, Email = token.Email}
            };
        }
    }

    public interface ISnsFactory
    {
        ContactDetailsSns Create(ContactDetailsRequestObject person, Token token);
    }

    public class ContactDetailsSns
    {
        public Guid Id { get; set; }

        public string EventType { get; set; }

        public string SourceDomain { get; set; }

        public string SourceSystem { get; set; }

        public string Version { get; set; }

        public Guid CorrelationId { get; set; }

        public DateTime DateTime { get; set; }

        public User User { get; set; }

        public Guid EntityId { get; set; }
    }

    public static class EntityFactory
    {
        public static ContactDetails ToDomain(this ContactDetailsEntity databaseEntity)
        {
            return new ContactDetails
            {
                Id = databaseEntity.Id,
                TargetId = databaseEntity.TargetId,
                TargetType = databaseEntity.TargetType,
                ContactInformation = databaseEntity.ContactInformation,
                SourceServiceArea = databaseEntity.SourceServiceArea,
                CreatedBy = databaseEntity.CreatedBy,
                IsActive = databaseEntity.IsActive,
                RecordValidUntil = databaseEntity.RecordValidUntil
            };
        }

        public static List<ContactDetails> ToDomain(this IEnumerable<ContactDetailsEntity> databaseEntity)
        {
            return databaseEntity.Select(p => p.ToDomain()).ToList();
        }

        public static ContactDetailsEntity ToDatabase(this ContactDetails entity)
        {
            return new ContactDetailsEntity
            {
                Id = entity.Id,
                TargetId = entity.TargetId,
                TargetType = entity.TargetType,
                ContactInformation = entity.ContactInformation,
                SourceServiceArea = entity.SourceServiceArea,
                CreatedBy = entity.CreatedBy,
                IsActive = entity.IsActive,
                RecordValidUntil = entity.RecordValidUntil
            };
        }

        public static ContactDetailsEntity ToDatabase(this ContactDetailsRequestObject entity)
        {
            return new ContactDetailsEntity
            {
                Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id,
                TargetId = entity.TargetId,
                TargetType = entity.TargetType,
                ContactInformation = entity.ContactInformation,
                SourceServiceArea = entity.SourceServiceArea,
                CreatedBy = entity.CreatedBy,
                IsActive = entity.IsActive,
                RecordValidUntil = entity.RecordValidUntil
            };
        }
    }
}
