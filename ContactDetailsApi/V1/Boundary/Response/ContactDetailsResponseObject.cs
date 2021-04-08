using ContactDetailsApi.V1.Domain;
using System;
using System.Collections.Generic;

namespace ContactDetailsApi.V1.Boundary.Response
{
    public class ContactDetailsResponseObject
    {
        public Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public TargetType TargetType { get; set; }
        public IEnumerable<ContactInformation> ContactInformation { get; set; }
        public IEnumerable<SourceServiceArea> SourceServiceArea { get; set; }
        public DateTime RecordValidUntil { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<CreatedBy> CreatedBy { get; set; }
    }
}
