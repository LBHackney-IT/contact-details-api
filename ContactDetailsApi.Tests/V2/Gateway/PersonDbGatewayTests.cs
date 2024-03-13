using AutoFixture;
using ContactDetailsApi.V2.Gateways;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Shared.Person.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xunit;
using FluentAssertions;
using System.Linq;
using Hackney.Core.Testing.Shared;

namespace ContactDetailsApi.Tests.V2.Gateway
{
    [Collection("AppTest collection")]
    public class PersonDbGatewayTests : IDisposable
    {
        private readonly Mock<ILogger<PersonDbGateway>> _logger;
        private readonly IDynamoDbFixture _dbFixture;
        private readonly PersonDbGateway _classUnderTest;
        private readonly Fixture _fixture = new Fixture();
        private readonly List<Action> _cleanup = new List<Action>();

        public PersonDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _logger = new Mock<ILogger<PersonDbGateway>>();
            _dbFixture = appFactory.DynamoDbFixture;
            _classUnderTest = new PersonDbGateway(_dbFixture.DynamoDbContext, _logger.Object);
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
        private async Task InsertDataIntoDynamoDB(IEnumerable<PersonDbEntity> entities)
        {
            foreach (var entity in entities)
            {
                await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task BatchGetPersonsWorksAsExpected()
        {
            var persons = _fixture.Build<PersonDbEntity>()
                                  .Without(x => x.VersionNumber)
                                  .CreateMany(10)
                                  .ToList();
            await InsertDataIntoDynamoDB(persons).ConfigureAwait(false);
            var personIds = new List<Guid>();

            foreach (var person in persons)
            {
                personIds.Add(person.Id);
            }

            var result = await _classUnderTest.GetPersons(personIds).ConfigureAwait(false);
            result.Should().NotBeNullOrEmpty();
            result.Should().BeEquivalentTo(persons);
            result.Should().HaveCount(10);
            _logger.VerifyExact(LogLevel.Information, $"Calling IDynamoDBContext.BatchGetAsync for {personIds.Count} persons", Times.Once());
        }
    }
}
