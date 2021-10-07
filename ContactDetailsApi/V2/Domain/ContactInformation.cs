using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Domain;
using Hackney.Core.DynamoDb.Converters;

namespace ContactDetailsApi.V2.Domain
{
    public class ContactInformation
    {
        [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<ContactType>))]
        public ContactType ContactType { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<SubType>))]
        public SubType? SubType { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public AddressExtended AddressExtended { get; set; }
    }
}