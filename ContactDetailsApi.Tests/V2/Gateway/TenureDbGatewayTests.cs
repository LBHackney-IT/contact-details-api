using AutoFixture;
using ContactDetailsApi.V2.Gateways;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Hackney.Shared.Person.Infrastructure;
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
        private readonly IDynamoDbFixture _dbFixture;
        private readonly TenureDbGateway _classUnderTest;
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

        [Fact]
        public async Task GetAllTenuresWorksAsExpected()
        {
            var tenures = _fixture.Build<TenureInformationDb>()
                                  .Without(x => x.VersionNumber)
                                  .CreateMany(10)
                                  .ToList();
            await InsertDataIntoDynamoDB(tenures).ConfigureAwait(false);

            var result = await _classUnderTest.GetAllTenures().ConfigureAwait(false);
            result.Should().NotBeNullOrEmpty();
            result.Should().HaveCount(tenures.Count);
            result.Should().BeEquivalentTo(tenures);
            _logger.VerifyExact(LogLevel.Information, "Calling IDynamoDBContext.ScanAsync for all tenures", Times.Once());

        }
    }
}
