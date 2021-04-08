using System.Collections.Generic;
using System.Linq;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;

namespace ContactDetailsApi.V1.Factories
{
    public static class ResponseFactory
    {
        public static ContactDetailsResponseObject ToResponse(this ContactDetails domain)
        {
            if (null == domain) return null;

            return new ContactDetailsResponseObject
            {
                Id = domain.Id,
                TargetId = domain.TargetId,
                TargetType = domain.TargetType,
                ContactInformation = domain.ContactInformation,
                CreatedBy = domain.CreatedBy,
                IsActive = domain.IsActive,
                RecordValidUntil = domain.RecordValidUntil,
                SourceServiceArea = domain.SourceServiceArea
            };
        }

        public static List<ContactDetailsResponseObject> ToResponse(this IEnumerable<ContactDetails> domainList)
        {
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }
    }
}
