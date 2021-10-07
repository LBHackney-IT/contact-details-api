using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using Hackney.Core.JWT;
using System;

namespace ContactDetailsApi.V2.Factories
{
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
                RecordValidUntil = databaseEntity.RecordValidUntil,
                LastModified = databaseEntity.LastModified
            };
        }

        public static ContactDetails ToDomain(this ContactDetailsRequestObject entity, Token token)
        {
            return new ContactDetails
            {
                Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id,
                TargetId = entity.TargetId,
                TargetType = entity.TargetType,
                ContactInformation = entity.ContactInformation,
                SourceServiceArea = entity.SourceServiceArea,
                CreatedBy = token.ToCreatedBy(),
                IsActive = true,
                RecordValidUntil = entity.RecordValidUntil
            };
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
                RecordValidUntil = entity.RecordValidUntil,
                LastModified = entity.LastModified
            };
        }
    }
}
