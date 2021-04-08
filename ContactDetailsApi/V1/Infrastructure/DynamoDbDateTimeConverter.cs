using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using System;


namespace ContactDetailsApi.V1.Infrastructure
{
    public class DynamoDbDateTimeConverter : IPropertyConverter
    {
        public static readonly string DATEFORMAT = "yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ";

        public DynamoDBEntry ToEntry(object value)
        {
            if (null == value) return new DynamoDBNull();

            return new Primitive { Value = ((DateTime) value).ToUniversalTime().ToString(DATEFORMAT) };
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            Primitive primitive = entry as Primitive;
            if (null == primitive) return (DateTime?) null;

            var dtString = primitive.Value.ToString();
            return DateTime.Parse(dtString, null, System.Globalization.DateTimeStyles.RoundtripKind);
        }
    }
}
