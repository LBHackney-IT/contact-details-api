using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace ContactDetailsApi.V2.Factories
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
            if (null == domainList) return new List<ContactDetailsResponseObject>();
            return domainList.Select(domain => domain.ToResponse()).ToList();
        }

        public static ContactsByUprnList ToResponse(this IEnumerable<ContactByUprn> domainList)
        {
            if (domainList == null) return new ContactsByUprnList();
            return new ContactsByUprnList
            {
                Results = domainList
            };
        }
    }
}
