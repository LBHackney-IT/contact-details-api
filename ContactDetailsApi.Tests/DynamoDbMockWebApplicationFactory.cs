using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Hackney.Core.DynamoDb;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ContactDetailsApi.Tests
{
    public class DynamoDbMockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly List<TableDef> _tables;

        public IAmazonDynamoDB DynamoDb { get; private set; }
        public IDynamoDBContext DynamoDbContext { get; private set; }

        public DynamoDbMockWebApplicationFactory(List<TableDef> tables)
        {
            _tables = tables;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<Startup>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureDynamoDB();

                var serviceProvider = services.BuildServiceProvider();
                DynamoDb = serviceProvider.GetRequiredService<IAmazonDynamoDB>();
                DynamoDbContext = serviceProvider.GetRequiredService<IDynamoDBContext>();

                EnsureTablesExist(DynamoDb, _tables);
            });
        }

        private static void EnsureTablesExist(IAmazonDynamoDB dynamoDb, List<TableDef> tables)
        {
            foreach (var table in tables)
            {
                try
                {
                    var keySchema = new List<KeySchemaElement> { new KeySchemaElement(table.KeyName, KeyType.HASH) };
                    var attributes = new List<AttributeDefinition> { new AttributeDefinition(table.KeyName, table.KeyType) };
                    if (!string.IsNullOrEmpty(table.RangeKeyName))
                    {
                        keySchema.Add(new KeySchemaElement(table.RangeKeyName, KeyType.RANGE));
                        attributes.Add(new AttributeDefinition(table.RangeKeyName, table.RangeKeyType));
                    }

                    var request = new CreateTableRequest(
                        table.Name,
                        keySchema,
                        attributes,
                        new ProvisionedThroughput(3, 3));
                    _ = dynamoDb.CreateTableAsync(request).GetAwaiter().GetResult();
                }
                catch (ResourceInUseException)
                {
                    // It already exists :-)
                }
            }
        }
    }
}
