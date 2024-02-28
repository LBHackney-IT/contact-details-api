using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Gateways
{
    [Collection("AppTest collection")]
    public class ContactDetailsDynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<ILogger<ContactDetailsDynamoDbGateway>> _logger;
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ContactDetailsDynamoDbGateway _classUnderTest;
        private readonly List<Action> _cleanup = new List<Action>();

        public ContactDetailsDynamoDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _logger = new Mock<ILogger<ContactDetailsDynamoDbGateway>>();
            _dbFixture = appFactory.DynamoDbFixture;
            _classUnderTest = new ContactDetailsDynamoDbGateway(_dbFixture.DynamoDbContext, _logger.Object);
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
            await _dbFixture.SaveEntityAsync<ContactDetailsEntity>(entity).ConfigureAwait(false);
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
                                 .With(x => x.LastModified, validDate)
                                 .Create();
            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);
            result.Should().HaveCount(1);
            result.First().Should().BeEquivalentTo(entity);
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {entity.TargetId}", Times.Once());
            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task DeleteContactDetailsByTargetIdSuccessfullySoftDeletes(bool hasValidDate)
        {
            DateTime? validDate = null;
            if (hasValidDate)
                validDate = DateTime.UtcNow;

            var entity = _fixture.Build<ContactDetailsEntity>()
                                 .With(x => x.RecordValidUntil, validDate)
                                 .With(x => x.IsActive, false)
                                 .Create();
            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new DeleteContactQueryParameter
            {
                TargetId = entity.TargetId,
                Id = entity.Id
            };
            var result = await _classUnderTest.DeleteContactDetailsById(query).ConfigureAwait(false);
            var load = await _dbFixture.DynamoDbContext.LoadAsync<ContactDetailsEntity>(query.TargetId, query.Id).ConfigureAwait(false);
            result.Should().BeEquivalentTo(load, config => config.Excluding(y => y.LastModified));
            load.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.LoadAsync for targetId {query.TargetId} and id {query.Id}", Times.Once());
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for targetId {query.TargetId} and id {query.Id}", Times.Once());
            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
        }

        [Fact]
        public async Task CreateContactDetailsCreatesRecord()
        {
            var entity = _fixture.Build<ContactDetailsEntity>()
                                 .With(x => x.RecordValidUntil, DateTime.UtcNow)
                                 .With(x => x.IsActive, true)
                                 .Create();

            var result = await _classUnderTest.CreateContact(entity).ConfigureAwait(false);
            result.Should().BeEquivalentTo(entity);

            var load = await _dbFixture.DynamoDbContext.LoadAsync<ContactDetailsEntity>(entity.TargetId, entity.Id).ConfigureAwait(false);
            result.Should().BeEquivalentTo(load, config => config.Excluding(y => y.LastModified));
            load.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);

            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for targetId {entity.TargetId} and id {entity.Id}", Times.Once());
        }
    }
}
