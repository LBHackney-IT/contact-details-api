using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.Tests.V1.Helper;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DynamoDbGatewayTests
    {
        //private readonly Fixture _fixture = new Fixture();
        private Mock<IDynamoDBContext> _dynamoDb;
        private DynamoDbGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _dynamoDb = new Mock<IDynamoDBContext>();
            _classUnderTest = new DynamoDbGateway(_dynamoDb.Object);
        }

        [Ignore("TO DO")]
        [Test]
        public void GetContactByIdReturnsNullIfEntityDoesntExist()
        {
            var response = _classUnderTest.GetEntityById(Guid.NewGuid());

            response.Should().BeNull();
        }

        //[Test]
        //public void GetContactByIdReturnsTheEntityIfItExists()
        //{
        //    var entity = _fixture.Create<ContactDetails>();
        //    var dbEntity = entity.ToDatabase();
        //    var dbIdUsed = entity.TargetId;

        //    _dynamoDb.Setup(x => x.LoadAsync<ContactDetailsEntity>(dbIdUsed, default))
        //             .ReturnsAsync(dbEntity);

        //    var response =  _classUnderTest.GetEntityById(entity.TargetId).ConfigureAwait(false);

        //    _dynamoDb.Verify(x => x.LoadAsync<ContactDetailsEntity>(dbIdUsed, default), Times.Once);
        //    entity.TargetId.Should().Be(response.targetId);
        //}
    }
}
