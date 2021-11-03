using Amazon.DynamoDBv2.DataModel;
using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Gateways;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private async Task InsertDataIntoDynamoDB(ContactDetailsEntity entity)
        {
            await _dynamoDb.SaveAsync<ContactDetailsEntity>(entity).ConfigureAwait(false);
            _cleanup.Add(async () => await _dynamoDb.DeleteAsync(entity).ConfigureAwait(false));
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

        [Fact]
        public async Task GetContactDetailsByTargetIdReturnsEmptyIfEntityDoesntExist()
        {
            // Arrange
            var targetId = Guid.NewGuid();
            var query = new ContactQueryParameter { TargetId = targetId };

            // Act
            var response = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {targetId}", Times.Once());
            response.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetContactDetailsByTargetIdReturnsExpectedContactDetails(bool hasValidDate)
        {
            // Arrange
            DateTime? validDate = null;
            if (hasValidDate) validDate = DateTime.UtcNow;

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.RecordValidUntil, validDate)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, validDate)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(1);

            result.First().Should().BeEquivalentTo(entity, config =>
            {
                return config.Excluding(x => x.ContactInformation);
            });

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {entity.TargetId}", Times.Once());
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdWhenAddressLine1IsEmptyAndContactTypeIsAddressReturnsContentsOfValue()
        {
            // Arrange
            var addressExtended = _fixture.Build<AddressExtended>()
                .With(x => x.AddressLine1, string.Empty)
                .Create();

            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.AddressExtended, addressExtended)
                .With(x => x.ContactType, ContactDetailsApi.V1.Domain.ContactType.address)
                .Create();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.First().ContactInformation.AddressExtended.AddressLine1.Should().Be(contactInformation.Value);

            result.First().Should().BeEquivalentTo(entity, config => config.Excluding(x => x.ContactInformation));
            result.First().ContactInformation.Should().BeEquivalentTo(contactInformation, config => config.Excluding(x => x.AddressExtended));
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdWhenAddressLine1NotEmptyReturnsMultilineAddressFields()
        {
            // Arrange
            var contactInformation = _fixture.Create<ContactInformation>();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.First().ContactInformation.AddressExtended.AddressLine1.Should().NotBe(contactInformation.Value);

            result.Should().BeEquivalentTo(entity);

        }
    }
}
