using System;
using ContactDetailsApi.V1.Domain;

namespace ContactDetailsApi.V1.Boundary.Request
{
    public class ContactDetailsRequestObject
    {
        public Guid Id { get; set; }

        public Guid TargetId { get; set; }

        public TargetType TargetType { get; set; }

        public ContactInformation ContactInformation { get; set; }

        public SourceServiceArea SourceServiceArea { get; set; }

        public DateTime? RecordValidUntil { get; set; }
    }
}
