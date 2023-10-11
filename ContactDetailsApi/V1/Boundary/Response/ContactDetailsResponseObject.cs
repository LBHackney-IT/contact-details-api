using ContactDetailsApi.V1.Domain;
using System;

namespace ContactDetailsApi.V1.Boundary.Response
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

        public int? VersionNumber { get; set; }
    }
}
