using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V1.E2ETests
{
    [Collection("DynamoDb collection")]
    public class DeleteContactDetailsE2ETests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();
        public ContactDetailsEntity ContactDetails { get; private set; }
        private readonly DynamoDbIntegrationTests<Startup> _dbFixture;
        private readonly List<Action> _cleanupActions = new List<Action>();

        public DeleteContactDetailsE2ETests(DynamoDbIntegrationTests<Startup> dbFixture)
        {
            _dbFixture = dbFixture;
        }
        private ContactDetails ConstructTestEntity()
        {
            var entity = _fixture.Create<ContactDetails>();
            return entity;
        }

        /// <summary>
        /// Method to add an entity instance to the database so that it can be used in a test.
        /// Also adds the corresponding action to remove the upserted data from the database when the test is done.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task SetupTestData(ContactDetails entity)
        {
            await _dbFixture.DynamoDbContext.SaveAsync(entity.ToDatabase()).ConfigureAwait(false);
            _cleanupActions.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync<ContactDetailsEntity>(entity.Id, entity.TargetId).ConfigureAwait(false));
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
                foreach (var action in _cleanupActions)
                    action();

                _disposed = true;
            }
        }

        [Fact]
        public async Task UpdatePersonByIdNotFoundReturns404()
        {
            var entity = ConstructTestEntity();
            var uri = new Uri($"api/v1/contactDetails/?targetId={entity.TargetId}", UriKind.Relative);
            var response = await _dbFixture.Client.GetAsync(uri).ConfigureAwait(false);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdatedPersonByIdFoundSuccessfullyUpdates()
        {
            var entity = ConstructTestEntity();
            await SetupTestData(entity).ConfigureAwait(false);
            var uri = new Uri($"api/v1/contactDetails/?targetId={entity.TargetId}", UriKind.Relative);
            var response = await _dbFixture.Client.GetAsync(uri).ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

