using AutoFixture;
using ContactDetailsApi;
using ContactDetailsApi.Tests;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.E2ETests
{

    public class E2EDynamoDbTest : DynamoDbIntegrationTests<Startup>
    {
        private readonly Fixture _fixture = new Fixture();

        /// <summary>
        /// Method to construct a test entity that can be used in a test
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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
            await DynamoDbContext.SaveAsync(entity.ToDatabase()).ConfigureAwait(false);
            CleanupActions.Add(async () => await DynamoDbContext.DeleteAsync<ContactDetailsEntity>(entity.TargetId).ConfigureAwait(false));
        }

        [Test]
        public async Task GetEntityByIdNotFoundReturns404()
        {
            var targetId = Guid.NewGuid().ToString();
            var uri = new Uri($"api/v1/contactDetails?targetId={targetId}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetNoteBydIdFoundReturnsResponse()
        {
            var entity = ConstructTestEntity();
            await SetupTestData(entity).ConfigureAwait(false);
            var targetId = entity.TargetId;
            var uri = new Uri($"api/v1/contactDetails?targetId={targetId}", UriKind.Relative);
            var response = await Client.GetAsync(uri).ConfigureAwait(false);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiEntity = JsonConvert.DeserializeObject<List<ContactDetails>>(responseContent);
            apiEntity.Should().BeEquivalentTo(entity);
        }
    }
}
