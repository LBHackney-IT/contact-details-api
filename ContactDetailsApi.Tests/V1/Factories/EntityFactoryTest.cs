using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using NUnit.Framework;
using System.Data;

namespace ContactDetailsApi.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();


        [Test]
        public void CanMapADatabaseEntityToADomainObject()
        {
            var databaseEntity = _fixture.Create<ContactDetailsEntity>();
            var entity = databaseEntity.ToDomain();

            databaseEntity.Id.Should().Be(entity.Id);
            databaseEntity.TargetId.Should().Be(entity.TargetId);
            databaseEntity.TargetType.Should().Be(entity.TargetType);
            databaseEntity.SourceServiceArea.Should().BeEquivalentTo(entity.SourceServiceArea);
            databaseEntity.RecordValidUntil.Should().Be(entity.RecordValidUntil);
            databaseEntity.IsActive.Should().Be(entity.IsActive);
            databaseEntity.CreatedBy.Should().BeEquivalentTo(entity.CreatedBy);
            databaseEntity.ContactInformation.Should().BeEquivalentTo(entity.ContactInformation);
        }


        [Test]
        public void CanMapADomainEntityToADatabaseObject()
        {
            var contactDetails = _fixture.Create<ContactDetails>();
            var databaseEntity = contactDetails.ToDatabase();

            contactDetails.Id.Should().Be(databaseEntity.Id);
            contactDetails.TargetId.Should().Be(databaseEntity.TargetId);
            contactDetails.TargetType.Should().Be(databaseEntity.TargetType);
            contactDetails.SourceServiceArea.Should().BeEquivalentTo(databaseEntity.SourceServiceArea);
            contactDetails.RecordValidUntil.Should().Be(databaseEntity.RecordValidUntil);
            contactDetails.IsActive.Should().Be(databaseEntity.IsActive);
            contactDetails.CreatedBy.Should().BeEquivalentTo(databaseEntity.CreatedBy);
            contactDetails.ContactInformation.Should().BeEquivalentTo(databaseEntity.ContactInformation);
        }
    }
}
