using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;

namespace ContactDetailsApi.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        //TODO: add assertions for all the fields being mapped in `EntityFactory.ToDomain()`. Also be sure to add test cases for
        // any edge cases that might exist.
        [Test]
        public void CanMapADatabaseEntityToADomainObject()
        {
            var databaseEntity = _fixture.Create<ContactDetailsEntity>();
            var entity = databaseEntity.ToDomain();

            databaseEntity.Id.Should().Be(entity.Id);
        }

        //TODO: add assertions for all the fields being mapped in `EntityFactory.ToDatabase()`. Also be sure to add test cases for
        // any edge cases that might exist.
        //[Test]
        //public void CanMapADomainEntityToADatabaseObject()
        //{
        //    var entity = _fixture.Create<ContactDetailsEntity>();
        //    var databaseEntity = entity.ToDatabase();

        //    entity.Id.Should().Be(databaseEntity.Id);
        //    entity.CreatedAt.Should().BeSameDateAs(databaseEntity.CreatedAt);
        //}
    }
}
