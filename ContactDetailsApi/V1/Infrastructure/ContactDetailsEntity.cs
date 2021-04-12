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


        [DynamoDBProperty(Converter = typeof(DynamoDbObjectListConverter<ContactInformation>))]

        public List<ContactInformation> ContactInformation { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbObjectListConverter<SourceServiceArea>))]
        public List<SourceServiceArea> SourceServiceArea { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime RecordValidUntil { get; set; }

        public bool IsActive { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbObjectListConverter<CreatedBy>))]

        public List<CreatedBy> CreatedBy { get; set; }
    }
}
