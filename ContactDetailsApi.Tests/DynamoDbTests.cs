using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace ContactDetailsApi.Tests
{
    [TestFixture]
    public class DynamoDbTests
    {
        protected IDynamoDBContext DynamoDbContext { get; private set; }
        protected AmazonDynamoDBClient DynamoDBClient { get; private set; }

        [SetUp]
        protected void RunBeforeTests()
        {
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;
            var clientConfig = new AmazonDynamoDBConfig { ServiceURL = "http://dynamodb-database:8000" };
            DynamoDBClient = new AmazonDynamoDBClient(clientConfig);
            try
            {
                var request = new CreateTableRequest("ContactDetails",
                new List<KeySchemaElement> { new KeySchemaElement("targetId", KeyType.HASH), new KeySchemaElement("id", KeyType.RANGE) },
                new List<AttributeDefinition> { new AttributeDefinition("targetId", ScalarAttributeType.S), new AttributeDefinition("id", ScalarAttributeType.S) },
                new ProvisionedThroughput(3, 3));

                DynamoDBClient.CreateTableAsync(request).GetAwaiter().GetResult();
            }
            catch (ResourceInUseException)
            {
                // It already exists :-)
            }
            DynamoDbContext = new DynamoDBContext(DynamoDBClient);
        }

        [TearDown]
        protected void RunAfterTests()
        {
            DynamoDBClient.Dispose();
        }
    }
}
