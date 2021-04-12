using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Factories
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
                ContactInformation = entity.ContactInformation.ToList(),
                SourceServiceArea = entity.SourceServiceArea.ToList(),
                CreatedBy = entity.CreatedBy.ToList(),
                IsActive = entity.IsActive,
                RecordValidUntil = entity.RecordValidUntil
            };
        }

        public static List<ContactDetailsEntity> ToDatabase(this IEnumerable<ContactDetails> entity)
        {
            return entity.Select(p => p.ToDatabase()).ToList();
        }
    }
}
