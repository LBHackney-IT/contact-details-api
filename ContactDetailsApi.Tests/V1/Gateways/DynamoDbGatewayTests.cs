using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [Collection("DynamoDb collection")]
    public class DynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<DynamoDbGateway>> _logger;
        private readonly IDynamoDBContext _dynamoDb;
        private readonly DynamoDbGateway _classUnderTest;
        private readonly List<Action> _cleanup = new List<Action>();

        public DynamoDbGatewayTests(DynamoDbIntegrationTests<Startup> dbTestFixture)
        {
            _logger = new Mock<ILogger<DynamoDbGateway>>();
            _dynamoDb = dbTestFixture.DynamoDbContext;
            _classUnderTest = new DynamoDbGateway(_dynamoDb, _logger.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                foreach (var action in _cleanup)
                    action();

                _disposed = true;
            }
        }

        private async Task InsertDataIntoDynamoDB(ContactDetailsEntity entity)
        {
            await _dynamoDb.SaveAsync<ContactDetailsEntity>(entity).ConfigureAwait(false);
            _cleanup.Add(async () => await _dynamoDb.DeleteAsync(entity).ConfigureAwait(false));
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdReturnsEmptyIfEntityDoesntExist()
        {
            var targetId = Guid.NewGuid();
            var query = new ContactQueryParameter { TargetId = targetId };
            var response = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {targetId}", Times.Once());
            response.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetContactDetailsByTargetIdReturnsExpectedContactDetails(bool hasValidDate)
        {
            DateTime? validDate = null;
            if (hasValidDate)
                validDate = DateTime.UtcNow;

            var entity = _fixture.Build<ContactDetailsEntity>()
                                 .With(x => x.RecordValidUntil, validDate)
                                 .With(x => x.IsActive, true)
                                 .Create();
            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);
            result.Should().BeEquivalentTo(entity);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {entity.TargetId}", Times.Once());
        }
    }
}
