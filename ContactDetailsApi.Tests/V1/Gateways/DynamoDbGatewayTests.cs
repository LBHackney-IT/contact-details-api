using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.Tests.V1.Helper;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DynamoDbGatewayTests
    {
        private readonly Fixture _fixture = new Fixture();
        private Mock<IDynamoDBContext> _dynamoDb;
        private DynamoDbGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _dynamoDb = new Mock<IDynamoDBContext>();
            _classUnderTest = new DynamoDbGateway(_dynamoDb.Object);
        }

        [Test]
        public async Task GetContactByTargetidReturnsNullIfEntityDoesntExist()
        {

            var response = await _classUnderTest.GetContactByTargetId(Guid.NewGuid()).ConfigureAwait(false);

            response.Should().BeEmpty();
        }

        [Test]
        public async Task VerifiesQueryByAsyncIsCalled()
        {
            var entity = _fixture.Create<ContactDetails>();
            var dbEntity = entity.ToDatabase();
            var dbIdUsed = entity.TargetId;

            var response = await _classUnderTest.GetContactByTargetId(entity.TargetId).ConfigureAwait(false);
            _dynamoDb.Verify(x => x.QueryAsync<ContactDetailsEntity>(dbIdUsed, default), Times.Once);
        }
    }
}
