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
    public class DynamoDbGatewayTests : DynamoDbTests
    {
        private readonly Fixture _fixture = new Fixture();
        private DynamoDbGateway _classUnderTest;
        private Mock<ILogger<DynamoDbGateway>> _logger;
        private LogCallAspectFixture _logCallAspectFixture;

        [SetUp]
        public void Setup()
        {
            _logCallAspectFixture = new LogCallAspectFixture();
            _logCallAspectFixture.RunBeforeTests();
            _logger = new Mock<ILogger<DynamoDbGateway>>();
            _classUnderTest = new DynamoDbGateway(DynamoDbContext, _logger.Object);
        }

        [Test]
        public async Task GetContactByTargetidReturnsEmptyIfEntityDoesntExist()
        {
            var targetId = Guid.NewGuid();
            var response = await _classUnderTest.GetContactByTargetId(targetId).ConfigureAwait(false);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId parameter {targetId}", Times.Once());
            response.Should().BeEmpty();
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
            DynamoDbContext.SaveAsync<ContactDetailsEntity>(entity).GetAwaiter().GetResult();
            CleanupActions.Add(async () => await DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
        }
    }
}
