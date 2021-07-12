using System;

namespace ContactDetailsApi.V1.Domain.Sns
{
    public class ContactDetailsSns
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public string SourceDomain { get; set; }
        public string SourceSystem { get; set; }
        public string Version { get; set; }
        public string CorrelationId { get; set; }
        public DateTime DateTime { get; set; }
        public User User { get; set; }
        public Guid EntityId { get; set; }
        public EventData EventData { get; set; }
    }
}
