using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Domain;

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
    }
}
