using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Domain;
using Hackney.Core.DynamoDb.Converters;
using System;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.V2.Infrastructure
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

        [DynamoDBProperty(Converter = typeof(DynamoDbObjectConverter<ContactInformation>))]
        public ContactInformation ContactInformation { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbObjectConverter<SourceServiceArea>))]
        public SourceServiceArea SourceServiceArea { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime? RecordValidUntil { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbBoolConverter))]
        public bool IsActive { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbObjectConverter<CreatedBy>))]
        public CreatedBy CreatedBy { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime? LastModified { get; set; }
    }
}
