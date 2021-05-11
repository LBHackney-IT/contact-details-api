using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Infrastructure;
using System;

namespace ContactDetailsApi.V1.Domain
{
    public class CreatedBy
    {
        public Guid Id { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        public string FullName { get; set; }

        public string EmailAddress { get; set; }
    }
}
