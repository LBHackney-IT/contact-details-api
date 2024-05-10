using AutoFixture;
using ContactDetailsApi.V2.Gateways;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Hackney.Shared.Tenure.Factories;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Gateway
{
    [Collection("AppTest collection")]
    public class TenureDbGatewayTests : IDisposable
    {
        private readonly Mock<ILogger<TenureDbGateway>> _logger;
        private readonly TenureDbGateway _classUnderTest;
        private readonly IDynamoDbFixture _dbFixture;
        private readonly Fixture _fixture = new Fixture();
        private readonly List<Action> _cleanup = new List<Action>();

        public TenureDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _logger = new Mock<ILogger<TenureDbGateway>>();
            _dbFixture = appFactory.DynamoDbFixture;
            _classUnderTest = new TenureDbGateway(_dbFixture.DynamoDbContext, _logger.Object);
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
        private async Task InsertDataIntoDynamoDB(IEnumerable<TenureInformationDb> entities)
        {
            foreach (var entity in entities)
            {
                await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
            }
        }

        private async Task DeleteDataInDynamoDB(IEnumerable<TenureInformationDb> entities)
        {
            foreach (var entity in entities)
            {
                await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ScanTenuresReturnsEmptyListWhenNoData()
        {
            var response = await _classUnderTest.ScanTenures(null, null).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().BeEmpty();
            response.PaginationDetails.HasNext.Should().BeFalse();
            response.PaginationDetails.NextToken.Should().BeNull();

            _logger.VerifyExact(LogLevel.Information, "Calling IDynamoDBContext.ScanAsync for TenureInformationDb", Times.Once());
        }

        [Fact]
        public async Task ScanTenuresReturnsDataWhenExists()
        {
            // Arrange
            var tenures = _fixture.Build<TenureInformationDb>()
                                  .Without(x => x.VersionNumber)
                                  .CreateMany(2)
                                  .ToList();
            await InsertDataIntoDynamoDB(tenures).ConfigureAwait(false);


            // Act
            var response = await _classUnderTest.ScanTenures(null, null).ConfigureAwait(false);

            // Assert
            response.Should().NotBeNull();
            response.Results.Should().NotBeNullOrEmpty();
            response.Results.Should().HaveCount(tenures.Count);
            response.Results.Should().BeEquivalentTo(tenures.Select(x => x.ToDomain()));
            response.PaginationDetails.HasNext.Should().BeFalse();
            response.PaginationDetails.NextToken.Should().BeNull();
            _logger.VerifyExact(LogLevel.Information, "Calling IDynamoDBContext.ScanAsync for TenureInformationDb", Times.Once());
            await DeleteDataInDynamoDB(tenures).ConfigureAwait(false);
        }

        [Fact]
        public async Task ScanTenuresReturnsDataWithPaginationToken()
        {
            // Arrange
            var tenures = _fixture.Build<TenureInformationDb>()
                                  .Without(x => x.VersionNumber)
                                  .CreateMany(9)
                                  .ToList();
            await InsertDataIntoDynamoDB(tenures).ConfigureAwait(false);

            // Act (1)
            var firstResponse = await _classUnderTest.ScanTenures(null, 5).ConfigureAwait(false);
            // Assert (1)
            firstResponse.Should().NotBeNull();
            firstResponse.Results.Should().NotBeNullOrEmpty();
            firstResponse.Results.Should().HaveCount(5);
            firstResponse.PaginationDetails.HasNext.Should().BeTrue();
            firstResponse.PaginationDetails.NextToken.Should().NotBeNullOrEmpty();

            // Arrange (2)
            var paginationToken = firstResponse.PaginationDetails.NextToken;

            // Act (2)
            var response = await _classUnderTest.ScanTenures(paginationToken, 5).ConfigureAwait(false);
            // Assert (2)
            response.Should().NotBeNull();
            response.Results.Should().NotBeNullOrEmpty();
            response.Results.Should().HaveCount(4);
            response.Results.Should().NotIntersectWith(firstResponse.Results);

            response.PaginationDetails.HasNext.Should().BeFalse();
            response.PaginationDetails.NextToken.Should().BeNull();

            _logger.VerifyExact(LogLevel.Information, "Calling IDynamoDBContext.ScanAsync for TenureInformationDb", Times.Exactly(2));
            await DeleteDataInDynamoDB(tenures).ConfigureAwait(false);

        }
    }
}
