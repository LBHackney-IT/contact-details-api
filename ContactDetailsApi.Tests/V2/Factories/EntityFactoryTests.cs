using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;
using Xunit;
using AddressExtended = ContactDetailsApi.V2.Domain.AddressExtended;
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
            var response = databaseEntity.ToDomain();

            // Assert
            response.Id.Should().Be(databaseEntity.Id);
            response.TargetId.Should().Be(databaseEntity.TargetId);
            response.TargetType.Should().Be(databaseEntity.TargetType);
            response.SourceServiceArea.Should().BeEquivalentTo(databaseEntity.SourceServiceArea);
            response.RecordValidUntil.Should().Be(databaseEntity.RecordValidUntil);
            response.IsActive.Should().Be(databaseEntity.IsActive);
            response.CreatedBy.Should().BeEquivalentTo(databaseEntity.CreatedBy);
            response.ContactInformation.Should().BeEquivalentTo(databaseEntity.ContactInformation);
            response.LastModified.Should().Be(databaseEntity.LastModified);

            var addressExtendedResponse = response.ContactInformation.AddressExtended;
            addressExtendedResponse.AddressLine1.Should().Be(databaseEntity.ContactInformation.AddressExtended.AddressLine1);
            addressExtendedResponse.AddressLine2.Should().Be(databaseEntity.ContactInformation.AddressExtended.AddressLine2);
            addressExtendedResponse.AddressLine3.Should().Be(databaseEntity.ContactInformation.AddressExtended.AddressLine3);
            addressExtendedResponse.AddressLine4.Should().Be(databaseEntity.ContactInformation.AddressExtended.AddressLine4);
            addressExtendedResponse.PostCode.Should().Be(databaseEntity.ContactInformation.AddressExtended.PostCode);
        }

        [Fact]
        public void ContactDetailsEntityToDomainWhenAddressLine1IsEmptyReturnsContentsOfValueField()
        {
            // Arrange
            var addressExtended = _fixture.Build<AddressExtended>()
                .With(x => x.AddressLine1, "")
                .Create();

            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.ContactType, ContactType.address)
                .With(x => x.AddressExtended, addressExtended)
                .Create();

            var databaseEntity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .Create();

            // Act
            var response = databaseEntity.ToDomain();

            // Assert
            response.ContactInformation.AddressExtended.AddressLine1.Should().Be(contactInformation.Value);
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
            databaseEntity.ContactInformation.Value.Should().Be(contactInformation.Value);

            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine1);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine2);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine3);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.AddressLine4);
            databaseEntity.ContactInformation.Value.Should().NotContain(contactInformation.AddressExtended.PostCode);
        }

        [Theory]
        [InlineData("line1", "", "", "", "pc", "line1 pc")]
        [InlineData("line1", "line2", "", "", "pc", "line1 line2 pc")]
        [InlineData("line1", "line2", "line3", "", "pc", "line1 line2 line3 pc")]
        [InlineData("line1", "line2", "line3", "line4", "pc", "line1 line2 line3 line4 pc")]
        public void ContactDetailsToDatabaseOnlyIncludesNotEmptyFieldsInValue(
            string addressLine1,
            string addressLine2,
            string addressLine3,
            string addressLine4,
            string postCode,
            string expectedFormat)
        {
            // Arrange
            var addressExtended = _fixture.Build<ContactDetailsApi.V2.Domain.AddressExtended>()
               .With(x => x.AddressLine1, addressLine1)
               .With(x => x.AddressLine2, addressLine2)
               .With(x => x.AddressLine3, addressLine3)
               .With(x => x.AddressLine4, addressLine4)
               .With(x => x.PostCode, postCode)
               .Create();

            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.ContactType, ContactType.address)
                .With(x => x.AddressExtended, addressExtended)
                .Create();

            var contactDetails = _fixture.Build<ContactDetails>()
                .With(x => x.ContactInformation, contactInformation)
                .Create();

            // Act
            var databaseEntity = contactDetails.ToDatabase();

            // Assert
            databaseEntity.ContactInformation.Value.Should().Be(expectedFormat);
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

        [Fact]
        public void CanMapADbEntityCollectionToAnOrderedDomainObjectCollection()
        {
            // Arrange
            var databaseEntities = CreateManyContactDetailsWithRandomDates(10);

            // Act
            var entities = databaseEntities.ToDomain();

            // Assert
            entities.Should().BeEquivalentTo(databaseEntities);
            entities.Should().BeInAscendingOrder(x => x.CreatedBy.CreatedAt);
        }

        private List<ContactDetailsEntity> CreateManyContactDetailsWithRandomDates(int quantity)
        {
            var random = new Random();

            var databaseEntities = new List<ContactDetailsEntity>();

            for (int i = 0; i < quantity; i++)
            {
                var randomDate = DateTime.UtcNow.AddHours(random.Next(500));

                var createdBy = _fixture.Build<CreatedBy>()
                    .With(y => y.CreatedAt, randomDate)
                    .Create();

                var entity = _fixture.Build<ContactDetailsEntity>()
                    .With(x => x.CreatedBy, createdBy)
                    .Create();

                databaseEntities.Add(entity);
            }

            return databaseEntities;
        }

        [Fact]
        public void CanMapContactDetailsEntityToDomainObject()
        {
            // Arrange
            var databaseEntity = _fixture.Create<ContactDetailsEntity>();

            // Act
            var entity = databaseEntity.ToDomain();

            // Assert
            entity.Id.Should().Be(databaseEntity.Id);
            entity.TargetId.Should().Be(databaseEntity.TargetId);
            entity.TargetType.Should().Be(databaseEntity.TargetType);
            entity.ContactInformation.Should().Be(databaseEntity.ContactInformation);
            entity.SourceServiceArea.Should().Be(databaseEntity.SourceServiceArea);
            entity.CreatedBy.Should().Be(databaseEntity.CreatedBy);
            entity.IsActive.Should().Be(databaseEntity.IsActive);
            entity.RecordValidUntil.Should().Be(databaseEntity.RecordValidUntil);
            entity.LastModified.Should().Be(databaseEntity.LastModified);
        }

        [Fact]
        public void CanMapContactDetailsRequestObjectToDomainObject()
        {
            // Arrange
            var entity = _fixture.Create<ContactDetailsRequestObject>();
            var token = _fixture.Create<Token>();

            // Act
            var domainEntity = entity.ToDomain(token);

            // Assert
            domainEntity.Id.Should().NotBe(Guid.Empty);
            domainEntity.TargetId.Should().Be(entity.TargetId);
            domainEntity.TargetType.Should().Be(entity.TargetType);
            domainEntity.ContactInformation.Should().Be(entity.ContactInformation);
            domainEntity.SourceServiceArea.Should().Be(entity.SourceServiceArea);
            domainEntity.CreatedBy.Should().Be(token.ToCreatedBy());
            domainEntity.IsActive.Should().BeTrue();
            domainEntity.RecordValidUntil.Should().Be(entity.RecordValidUntil);
            domainEntity.LastModified.Should().BeNull();
        }

        [Fact]
        public void CanMapEditContactDetailsRequestToEditContactDetailsDatabase()
        {
            // Arrange
            var request = _fixture.Create<EditContactDetailsRequest>();

            // Act
            var databaseEntity = request.ToDatabase();

            // Assert
            databaseEntity.ContactInformation.Should().Be(request.ContactInformation);
            databaseEntity.LastModified.Should().BeCloseTo(DateTime.UtcNow, 1000);
        }

        [Fact]
        public void CanMapContactDetailsToContactDetailsEntity()
        {
            // Arrange
            var domain = _fixture.Create<ContactDetails>();

            // Act
            var databaseEntity = domain.ToDatabase();

            // Assert
            databaseEntity.Id.Should().Be(domain.Id);
            databaseEntity.TargetId.Should().Be(domain.TargetId);
            databaseEntity.TargetType.Should().Be(domain.TargetType);
            databaseEntity.ContactInformation.Should().Be(domain.ContactInformation);
            databaseEntity.SourceServiceArea.Should().Be(domain.SourceServiceArea);
            databaseEntity.CreatedBy.Should().Be(domain.CreatedBy);
            databaseEntity.IsActive.Should().Be(domain.IsActive);
            databaseEntity.RecordValidUntil.Should().Be(domain.RecordValidUntil);
            databaseEntity.LastModified.Should().Be(domain.LastModified);
        }

        [Fact]
        public void CanMapContactDetailsEntityCollectionToDomainObjectCollection()
        {
            // Arrange
            var databaseEntities = _fixture.CreateMany<ContactDetailsEntity>(10);

            // Act
            var entities = databaseEntities.ToDomain();

            // Assert
            entities.Should().BeEquivalentTo(databaseEntities);
            entities.Should().BeInAscendingOrder(x => x.CreatedBy.CreatedAt);
        }

        // ServiceSoft Endpoint Factories

        [Fact]
        public void CanMapContactDetailsToPersonContactDetails()
        {
            // Arrange
            var databaseEntity = _fixture.Create<ContactDetails>();

            // Act
            var personContactDetails = databaseEntity.ToUprnContact();

            // Assert
            personContactDetails.ContactType.Should().Be(databaseEntity.ContactInformation.ContactType);
            personContactDetails.SubType.Should().Be(databaseEntity.ContactInformation.SubType);
            personContactDetails.Value.Should().Be(databaseEntity.ContactInformation.Value);
        }

        [Fact]
        public void CanMapContactDetailsCollectionToPersonContactDetailsList()
        {
            // Arrange
            var databaseEntities = _fixture.CreateMany<ContactDetails>(10);

            // Act
            var personContactDetailsList = databaseEntities.ToContactByUprnPersonContacts();

            // Assert
            personContactDetailsList.Should().BeEquivalentTo(databaseEntities, options => options.ExcludingMissingMembers());
        }

        [Fact]
        public void CanMapPersonDetailsToPerson()
        {
            // Arrange
            var personDetails = _fixture.Create<Hackney.Shared.Person.Person>();
            var householdMember = _fixture.Create<HouseholdMembers>();
            var contactDetails = _fixture.Create<List<PersonContactDetails>>();

            // Act
            var person = personDetails.ToContactByUprnPerson(householdMember, contactDetails);

            // Assert
            person.PersonTenureType.Should().Be(householdMember.PersonTenureType);
            person.IsResponsible.Should().Be(householdMember.IsResponsible);
            person.FirstName.Should().Be(personDetails.FirstName);
            person.LastName.Should().Be(personDetails.Surname);
            person.Title.Should().Be(personDetails.Title);
            person.PersonContactDetails.Should().BeEquivalentTo(contactDetails);
        }

        [Fact]
        public void CanMapTenureInformationToContactByUprn()
        {
            // Arrange
            var tenure = _fixture.Create<TenureInformation>();
            var contacts = _fixture.Create<List<Person>>();

            // Act
            var contactByUprn = tenure.ToContactByUprn(contacts);

            // Assert
            contactByUprn.TenureId.Should().Be(tenure.Id);
            contactByUprn.Uprn.Should().Be(tenure.TenuredAsset?.Uprn);
            contactByUprn.Contacts.Should().BeEquivalentTo(contacts);
        }
    }
}
