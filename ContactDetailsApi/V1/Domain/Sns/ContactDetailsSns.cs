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

    public class DataItem
    {
        public string Value { get; set; }
    }
    public class EventData
    {
        public DataItem OldData { get; set; }
        public DataItem NewData { get; set; }
    }
}
