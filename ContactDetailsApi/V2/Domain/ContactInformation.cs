using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Domain;
using Hackney.Core.DynamoDb.Converters;

namespace ContactDetailsApi.V2.Domain
{
    public class ContactInformation
    {
        public ContactType ContactType { get; set; }

        public SubType? SubType { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public AddressExtended AddressExtended { get; set; }
    }
}
