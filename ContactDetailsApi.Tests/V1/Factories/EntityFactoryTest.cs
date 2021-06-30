using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Factories
{
    public class EntityFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
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

        [Fact]
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanMapARequestToADatabaseObject(bool hasId)
        {
            var id = hasId ? Guid.NewGuid() : Guid.Empty;
            var request = _fixture.Build<ContactDetailsRequestObject>()
                                         .With(x => x.Id, id)
                                         .Create();
            var databaseEntity = request.ToDatabase();

            if (hasId)
                request.Id.Should().Be(databaseEntity.Id);
            else
                databaseEntity.Id.Should().NotBeEmpty();
            request.TargetId.Should().Be(databaseEntity.TargetId);
            request.TargetType.Should().Be(databaseEntity.TargetType);
            request.SourceServiceArea.Should().BeEquivalentTo(databaseEntity.SourceServiceArea);
            request.RecordValidUntil.Should().Be(databaseEntity.RecordValidUntil);
            request.IsActive.Should().Be(databaseEntity.IsActive);
            request.CreatedBy.Should().BeEquivalentTo(databaseEntity.CreatedBy);
            request.ContactInformation.Should().BeEquivalentTo(databaseEntity.ContactInformation);
        }
    }
}
