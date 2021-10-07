using ContactDetailsApi.V1.Domain;
using System;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.V2.Boundary.Response
{
    public class ContactDetailsResponseObject
    {
        public Guid Id { get; set; }

        public Guid TargetId { get; set; }

        public TargetType TargetType { get; set; }

        public ContactInformation ContactInformation { get; set; }

        public SourceServiceArea SourceServiceArea { get; set; }

        public DateTime? RecordValidUntil { get; set; }

        public bool IsActive { get; set; }

        public CreatedBy CreatedBy { get; set; }
    }
}
