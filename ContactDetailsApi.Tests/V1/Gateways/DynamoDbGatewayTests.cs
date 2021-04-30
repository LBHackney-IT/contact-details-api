using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core.Sampling;
using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DynamoDbGatewayTests : LogCallAspectFixture
    {
        private readonly Fixture _fixture = new Fixture();
        private DynamoDbGateway _classUnderTest;
        private Mock<ILogger<DynamoDbGateway>> _logger;
        protected DynamoDbTests dynamoDbTests { get; set; }
        public void Setup()
        {
            _logger = new Mock<ILogger<DynamoDbGateway>>();
            _classUnderTest = new DynamoDbGateway(dynamoDbTests.DynamoDbContext, _logger.Object);
        }

        [Test]
        public async Task GetContactByTargetidReturnsEmptyIfEntityDoesntExist()
        {
            var targetId = Guid.NewGuid();
            var response = await _classUnderTest.GetContactByTargetId(targetId).ConfigureAwait(false);

            response.Should().BeEmpty();
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId parameter {targetId}", Times.Once());
        }

        [Test]
        public async Task VerifiesGatewayMethodsAddtoDB()
        {
            var entity = _fixture.Build<ContactDetailsEntity>().Create();
            InsertDatatoDynamoDB(entity);

            var result = await _classUnderTest.GetContactByTargetId(entity.TargetId).ConfigureAwait(false);
            result.Should().BeEquivalentTo(entity);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId parameter {entity.TargetId}", Times.Once());
        }

        private void InsertDatatoDynamoDB(ContactDetailsEntity entity)
        {
            dynamoDbTests.DynamoDbContext.SaveAsync<ContactDetailsEntity>(entity).GetAwaiter().GetResult();
        }
    }
}
