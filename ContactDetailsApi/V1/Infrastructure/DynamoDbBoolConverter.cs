using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace ContactDetailsApi.V1.Infrastructure
{
    // TODO: This should go in a common NuGet package...

    public class DynamoDbBoolConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            if (null == value) return new DynamoDBNull();

            return new DynamoDBBool((bool) value);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            if (entry is null) return null;
            if (entry is DynamoDBBool) return entry.AsBoolean();
            return bool.Parse(entry.AsString());
        }
    }
}
