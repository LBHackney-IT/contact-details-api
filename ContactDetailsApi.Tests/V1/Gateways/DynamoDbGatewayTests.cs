using Amazon.DynamoDBv2.Model;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DynamoDbGatewayTests : DynamoDbTests<Startup>
    {
        private readonly Fixture _fixture = new Fixture();
        private DynamoDbGateway _classUnderTest;


        [SetUp]
        public void Setup()
        {

            _classUnderTest = new DynamoDbGateway(DynamoDbContext);
        }

        [Test]
        public async Task GetContactByTargetidReturnsEmptyIfEntityDoesntExist()
        {
            
            var response = await _classUnderTest.GetContactByTargetId(Guid.NewGuid()).ConfigureAwait(false);

            response.Should().BeEmpty();
        }

        [Test]
        public async Task VerifiesGatewayMethodsAddtoDB()
        {
            var entity = _fixture.Build<ContactDetailsEntity>()
                                 .Create();
            InsertDatatoDynamoDB(entity);

            var result = await _classUnderTest.GetContactByTargetId(entity.TargetId).ConfigureAwait(false);
            result.Should().BeEquivalentTo(entity); 
        }

        private void InsertDatatoDynamoDB(ContactDetailsEntity entity)
        {
            //Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            //attributes["targetId"] = new AttributeValue { S = entity.TargetId.ToString() };
            //attributes["id"] = new AttributeValue { S = entity.Id.ToString() };
            //attributes["TargetType"] = new AttributeValue { S = entity.TargetType.ToString() };
            //attributes["ContactInformation"] = new AttributeValue { S = JsonConvert.SerializeObject(entity.ContactInformation) };
            //attributes["SourceServiceArea"] = new AttributeValue { S = JsonConvert.SerializeObject(entity.SourceServiceArea) };
            //attributes["RecordValidUntil"] = new AttributeValue { S = JsonConvert.SerializeObject(entity.RecordValidUntil) };
            //attributes["CreatedBy"] = new AttributeValue { S = JsonConvert.SerializeObject(entity.CreatedBy) };
            //attributes["IsActive"] = new AttributeValue { BOOL = entity.IsActive };

            //PutItemRequest request = new PutItemRequest
            //{
            //    TableName = "ContactDetails",
            //    Item = attributes
            //};
            //DynamoDBClient.PutItemAsync(request).GetAwaiter().GetResult();

          
            DynamoDbContext.SaveAsync<ContactDetailsEntity>(entity).GetAwaiter().GetResult();

        }
    }
}
