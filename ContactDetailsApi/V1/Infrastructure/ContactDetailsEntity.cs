using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContactDetailsApi.V1.Infrastructure
{

    [DynamoDBTable("ContactDetails", LowerCamelCaseProperties = true)]
    public class ContactDetailsEntity
    {
        [DynamoDBHashKey]
        public Guid TargetId { get; set; }

        [DynamoDBRangeKey]
        public Guid Id { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<TargetType>))]

        public TargetType TargetType { get; set; }

        public ContactInformation ContactInformation { get; set; }

        public SourceServiceArea SourceServiceArea { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime RecordValidUntil { get; set; }

        public bool IsActive { get; set; }

        public CreatedBy CreatedBy { get; set; }
    }
}
