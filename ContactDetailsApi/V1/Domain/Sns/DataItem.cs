using System;

namespace ContactDetailsApi.V1.Domain.Sns
{
    public class DataItem
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public ContactType ContactType { get; set; }
        public string Description { get; set; }
    }
}
