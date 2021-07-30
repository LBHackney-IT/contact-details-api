using Amazon.DynamoDBv2.DataModel;
using Hackney.Core.DynamoDb.Converters;
using System;

namespace ContactDetailsApi.V1.Domain
{
    public class CreatedBy
    {
        [DynamoDBProperty(Converter = typeof(DynamoDbDateTimeConverter))]
        public DateTime? CreatedAt { get; set; }

        public string FullName { get; set; }

        public string EmailAddress { get; set; }
    }
}
