using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V2.Gateways;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Gateway
{
    [Collection("Aws collection")]
    public class DynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<DynamoDbGateway>> _logger;
        private readonly IDynamoDBContext _dynamoDb;
        private readonly DynamoDbGateway _classUnderTest;
        private readonly List<Action> _cleanup = new List<Action>();

        public DynamoDbGatewayTests(AwsIntegrationTests<Startup> dbTestFixture)
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

        [Fact]
        public async Task CreateContactDetailsCreatesRecord()
        {
            // Arrange
            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .Create();

            // Act
            var result = await _classUnderTest.CreateContact(entity).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(entity);

            var databaseResponse = await _dynamoDb.LoadAsync<ContactDetailsEntity>(entity.TargetId, entity.Id).ConfigureAwait(false);

            databaseResponse.Should().NotBeNull();
            result.Should().BeEquivalentTo(databaseResponse, config => config.Excluding(y => y.LastModified));
            databaseResponse.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);

            _cleanup.Add(async () => await _dynamoDb.DeleteAsync(entity).ConfigureAwait(false));
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for targetId {entity.TargetId} and id {entity.Id}", Times.Once());
        }
    }
}
