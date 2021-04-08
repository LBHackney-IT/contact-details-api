

using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Infrastructure;

namespace ContactDetailsApi.V1.Domain
{
    public class ContactInformation
    {
        [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<ContactType>))]
        public ContactType ContactType { get; set; }

        [DynamoDBProperty(Converter = typeof(DynamoDbEnumConverter<SubType>))]
        public SubType SubType { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        [DynamoDBProperty(Converter = typeof(DynamoDbObjectListConverter<AddressExtended>))]
        public AddressExtended AddressExtended { get; set; }

    }
}
