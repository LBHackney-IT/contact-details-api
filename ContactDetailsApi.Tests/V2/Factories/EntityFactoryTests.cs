using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Core.JWT;
using System;
using Xunit;
using ContactDetails = ContactDetailsApi.V2.Domain.ContactDetails;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.Tests.V2.Factories
{
    public class EntityFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CanMapADatabaseEntityToADomainObject()
        {
            // Arrange
            var databaseEntity = _fixture.Create<ContactDetailsEntity>();

            // Act
            var entity = databaseEntity.ToDomain();

            // Assert
            databaseEntity.Id.Should().Be(entity.Id);
            databaseEntity.TargetId.Should().Be(entity.TargetId);
            databaseEntity.TargetType.Should().Be(entity.TargetType);
            databaseEntity.SourceServiceArea.Should().BeEquivalentTo(entity.SourceServiceArea);
            databaseEntity.RecordValidUntil.Should().Be(entity.RecordValidUntil);
            databaseEntity.IsActive.Should().Be(entity.IsActive);
            databaseEntity.CreatedBy.Should().BeEquivalentTo(entity.CreatedBy);
            databaseEntity.ContactInformation.Should().BeEquivalentTo(entity.ContactInformation);
            databaseEntity.LastModified.Should().Be(entity.LastModified);
        }

        [Fact]
        public void CanMapADomainEntityToADatabaseObject()
        {
            // Arrange
            var contactDetails = _fixture.Create<ContactDetails>();

            // Act
            var databaseEntity = contactDetails.ToDatabase();

            // Assert
            contactDetails.Id.Should().Be(databaseEntity.Id);
            contactDetails.TargetId.Should().Be(databaseEntity.TargetId);
            contactDetails.TargetType.Should().Be(databaseEntity.TargetType);
            contactDetails.SourceServiceArea.Should().BeEquivalentTo(databaseEntity.SourceServiceArea);
            contactDetails.RecordValidUntil.Should().Be(databaseEntity.RecordValidUntil);
            contactDetails.IsActive.Should().Be(databaseEntity.IsActive);
            contactDetails.CreatedBy.Should().BeEquivalentTo(databaseEntity.CreatedBy);
            contactDetails.ContactInformation.Should().BeEquivalentTo(databaseEntity.ContactInformation);
            contactDetails.LastModified.Should().Be(databaseEntity.LastModified);
        }

        [Fact]
        public void ContactDetailsToDatabaseWhenContactTypeIsAddressFormatsMultilineAddressIntoValueField()
        {
            // Arrange
            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.ContactType, ContactType.address)
                .Create();


            var contactDetails = _fixture.Build<ContactDetails>()
                                    .With(x => x.ContactInformation, contactInformation)
                                    .Create();
            // Act
            var databaseEntity = contactDetails.ToDatabase();

            // Assert
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.AddressExtended.AddressLine1);
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.AddressExtended.AddressLine2);
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.AddressExtended.AddressLine3);
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.AddressExtended.AddressLine4);
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.AddressExtended.PostCode);
        }

        [Fact]
        public void ContactDetailsToDatabaseWhenContactTypeIsNotAddressDoesntFormatMultilineAddressIntoValueField()
        {
            // Arrange
            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.ContactType, ContactType.phone)
                .Create();


            var contactDetails = _fixture.Build<ContactDetails>()
                                    .With(x => x.ContactInformation, contactInformation)
                                    .Create();
            // Act
            var databaseEntity = contactDetails.ToDatabase();

            // Assert
            databaseEntity.ContactInformation.Value.Should().Contain(contactInformation.Value);

            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine1);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine2);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine3);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine4);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.PostCode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanMapARequestToADomainObject(bool hasId)
        {
            // Arrange
            var id = hasId ? Guid.NewGuid() : Guid.Empty;
            var request = _fixture.Build<ContactDetailsRequestObject>().With(x => x.Id, id).Create();
            var token = _fixture.Create<Token>();

            // Act
            var domainEntity = request.ToDomain(token);

            // Assert
            if (hasId)
            {
                request.Id.Should().Be(domainEntity.Id);
            }
            else
            {
                domainEntity.Id.Should().NotBeEmpty();
                domainEntity.TargetId.Should().Be(request.TargetId);
                domainEntity.TargetType.Should().Be(request.TargetType);
                domainEntity.SourceServiceArea.Should().BeEquivalentTo(request.SourceServiceArea);
                domainEntity.RecordValidUntil.Should().Be(request.RecordValidUntil);
                domainEntity.IsActive.Should().BeTrue();
                domainEntity.CreatedBy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1000);
                domainEntity.CreatedBy.EmailAddress.Should().BeEquivalentTo(token.Email);
                domainEntity.CreatedBy.FullName.Should().BeEquivalentTo(token.Name);
                domainEntity.ContactInformation.Should().BeEquivalentTo(request.ContactInformation);
                domainEntity.LastModified.Should().BeNull();
            }
        }

        [Fact]
        public void CanMapATokenToACreatedBy()
        {
            // Arrange
            var token = _fixture.Create<Token>();

            // Act
            var createdBy = token.ToCreatedBy();

            // Assert
            createdBy.FullName.Should().Be(token.Name);
            createdBy.EmailAddress.Should().Be(token.Email);
            createdBy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 100);
        }
    }
}
