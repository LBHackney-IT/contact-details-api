using Amazon.DynamoDBv2;
using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Gateways;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using FluentAssertions;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Shared;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AddressExtended = ContactDetailsApi.V2.Domain.AddressExtended;
using ContactInformation = ContactDetailsApi.V2.Domain.ContactInformation;

namespace ContactDetailsApi.Tests.V2.Gateway
{
    [Collection("AppTest collection")]
    public class ContactDetailsDynamoDbGatewayTests : IDisposable
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<ILogger<ContactDetailsDynamoDbGateway>> _logger;
        private readonly Mock<IEntityUpdater> _updater;
        private readonly IDynamoDbFixture _dbFixture;
        private readonly ContactDetailsDynamoDbGateway _classUnderTest;
        private readonly List<Action> _cleanup = new List<Action>();
        private readonly IAmazonDynamoDB _amazonDynamoDb;

        public ContactDetailsDynamoDbGatewayTests(MockWebApplicationFactory<Startup> appFactory)
        {
            _logger = new Mock<ILogger<ContactDetailsDynamoDbGateway>>();
            _updater = new Mock<IEntityUpdater>();
            _dbFixture = appFactory.DynamoDbFixture;
            _amazonDynamoDb = appFactory.DynamoDbFixture.DynamoDb;
            _classUnderTest = new ContactDetailsDynamoDbGateway(_dbFixture.DynamoDbContext, _logger.Object, _updater.Object, _dbFixture.DynamoDb);
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

        private async Task InsertDataIntoDynamoDB(AssetDb entity)
        {
            await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
        }

        private async Task InsertDataIntoDynamoDB(IEnumerable<AssetDb> entities)
        {
            foreach (var entity in entities)
            {
                await InsertDataIntoDynamoDB(entity);
            }
        }

        private async Task InsertDataIntoDynamoDB(TenureInformationDb entity)
        {
            await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
        }

        private async Task InsertDataIntoDynamoDB(IEnumerable<TenureInformationDb> entities)
        {
            foreach (var entity in entities)
            {
                await InsertDataIntoDynamoDB(entity);
            }
        }

        private async Task InsertDataIntoDynamoDB(ContactDetailsEntity entity)
        {
            await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
        }

        private async Task InsertDataIntoDynamoDB(IEnumerable<ContactDetailsEntity> entities)
        {
            foreach (var entity in entities)
            {
                await InsertDataIntoDynamoDB(entity);
            }
        }


        private async Task InsertDataIntoDynamoDB(PersonDbEntity entity)
        {
            await _dbFixture.SaveEntityAsync(entity).ConfigureAwait(false);
        }

        private async Task InsertDataIntoDynamoDB(IEnumerable<PersonDbEntity> entities)
        {
            foreach (var entity in entities)
            {
                await InsertDataIntoDynamoDB(entity);
            }
        }


        [Fact]
        public async Task FetchAllContactDetailsByUprnWorksAsExpected()
        {
            // Arrange
            var person = _fixture.Build<PersonDbEntity>()
                .Without(x => x.VersionNumber)
                .Create();
            await InsertDataIntoDynamoDB(person).ConfigureAwait(false);

            var contactInformation = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.TargetId, person.Id)
                .CreateMany(2)
                .ToList();

            await InsertDataIntoDynamoDB(contactInformation).ConfigureAwait(false);

            var tenure = _fixture.Build<TenureInformationDb>()
                .Without(x => x.VersionNumber)
                .With(x => x.HouseholdMembers,
                    _fixture.Build<HouseholdMembers>()
                    .With(x => x.Id, person.Id)
                    .CreateMany(1)
                    .ToList()
                )
                .Create();

            await InsertDataIntoDynamoDB(tenure).ConfigureAwait(false);

            var asset = _fixture.Build<AssetDb>()
                .Without(x => x.VersionNumber)
                .Create();

            asset.Tenure.Id = tenure.Id.ToString();

            await InsertDataIntoDynamoDB(asset).ConfigureAwait(false);

            // Act
            var result = await _classUnderTest.FetchAllContactDetailsByUprnUseCase().ConfigureAwait(false);
            result.Should().NotBeNull();


            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(tenure).ConfigureAwait(false));
            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(asset).ConfigureAwait(false));
            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(person).ConfigureAwait(false));

            foreach (var contact in contactInformation)
            {
                _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(contact).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task FetchAllAssetsWorksAsExpected()
        {
            //Arrange
            var tenure = _fixture.Build<AssetTenureDb>().With(x => x.Id, Guid.NewGuid().ToString()).Create();
            var assets = _fixture.Build<AssetDb>()
                                 .Without(x => x.VersionNumber)
                                 .With(x => x.Tenure, tenure)
                                 .CreateMany(10)
                                 .ToList();
            await InsertDataIntoDynamoDB(assets).ConfigureAwait(false);

            //Act

            var result = await _classUnderTest.FetchAllAssets().ConfigureAwait(false);
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<ContactByUprn>>();
            result.Should().HaveCount(10);


            foreach (var asset in result)
            {
                _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(asset).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task FetchAllContactDetailsWorksAsExpected()
        {
            //Arrange
            var contactDetails = _fixture.Build<ContactDetailsEntity>()
                                 .CreateMany(10)
                                 .ToList();
            await InsertDataIntoDynamoDB(contactDetails).ConfigureAwait(false);

            //Act

            var result = await _classUnderTest.FetchAllContactDetails().ConfigureAwait(false);

            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<ContactDetailsEntity>>();
            result.Should().HaveCount(10);

            foreach (var contact in result)
            {
                _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(contact).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task FetchAllTenuresWorksAsExpected()
        {
            var tenures = _fixture.Build<TenureInformationDb>()
                                  .Without(x => x.VersionNumber)
                                  .CreateMany(10)
                                  .ToList();
            await InsertDataIntoDynamoDB(tenures).ConfigureAwait(false);
            var tenureIds = new List<Guid?>();

            foreach (var tenure in tenures)
            {
                tenureIds.Add(tenure.Id);
            }

            var result = await _classUnderTest.FetchTenures(tenureIds).ConfigureAwait(false);
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<TenureInformationDb>>();
            result.Should().HaveCount(10);

            foreach (var tenure in tenures)
            {
                _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(tenure).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task FetchPersonsWorksAsExpected()
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

            var result = await _classUnderTest.FetchPersons(personIds).ConfigureAwait(false);
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<PersonDbEntity>>();
            result.Should().HaveCount(10);

            foreach (var person in persons)
            {
                _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(person).ConfigureAwait(false));
            }
        }

        [Fact]
        public async Task CreateContactDetailsCreatesRecord()
        {
            // Arrange
            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .Create();

            // Act
            var result = await _classUnderTest.CreateContact(entity).ConfigureAwait(false);

            // Assert
            result.Should().BeEquivalentTo(entity, config => config);

            var databaseResponse = await _dbFixture.DynamoDbContext.LoadAsync<ContactDetailsEntity>(entity.TargetId, entity.Id).ConfigureAwait(false);

            databaseResponse.Should().NotBeNull();

            result.Should().BeEquivalentTo(databaseResponse, config =>
                config.Excluding(y => y.LastModified)

            );

            databaseResponse.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);

            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.SaveAsync for targetId {entity.TargetId} and id {entity.Id}", Times.Once());
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdReturnsEmptyIfEntityDoesntExist()
        {
            // Arrange
            var targetId = Guid.NewGuid();
            var query = new ContactQueryParameter { TargetId = targetId };

            // Act
            var response = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {targetId}", Times.Once());
            response.Should().BeEmpty();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetContactDetailsByTargetIdReturnsExpectedContactDetails(bool hasValidDate)
        {
            // Arrange
            DateTime? validDate = null;
            if (hasValidDate) validDate = DateTime.UtcNow;

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.RecordValidUntil, validDate)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, validDate)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.Should().HaveCount(1);

            result.First().Should().BeEquivalentTo(entity, config => config.Excluding(x => x.ContactInformation));

            _logger.VerifyExact(LogLevel.Debug, $"Calling IDynamoDBContext.QueryAsync for targetId {entity.TargetId}", Times.Once());
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdWhenAddressLine1IsEmptyAndContactTypeIsAddressReturnsContentsOfValue()
        {
            // Arrange
            var addressExtended = _fixture.Build<AddressExtended>()
                .With(x => x.AddressLine1, string.Empty)
                .Create();

            var contactInformation = _fixture.Build<ContactInformation>()
                .With(x => x.AddressExtended, addressExtended)
                .With(x => x.ContactType, ContactDetailsApi.V1.Domain.ContactType.address)
                .Create();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.First().ContactInformation.AddressExtended.AddressLine1.Should().Be(contactInformation.Value);

            result.First().Should().BeEquivalentTo(entity, config => config.Excluding(x => x.ContactInformation));
            result.First().ContactInformation.Should().BeEquivalentTo(contactInformation, config => config.Excluding(x => x.AddressExtended));
        }

        [Fact]
        public async Task GetContactDetailsByTargetIdWhenAddressLine1NotEmptyReturnsMultilineAddressFields()
        {
            // Arrange
            var contactInformation = _fixture.Create<ContactInformation>();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var query = new ContactQueryParameter { TargetId = entity.TargetId };

            // Act
            var result = await _classUnderTest.GetContactDetailsByTargetId(query).ConfigureAwait(false);

            // Assert
            result.First().ContactInformation.AddressExtended.AddressLine1.Should().NotBe(contactInformation.Value);

            result.First().Should().BeEquivalentTo(entity, options => options);
        }

        [Fact]
        public async Task EditContactDetails_WhenContactDoesntExist_ReturnsNull()
        {
            // Arrange
            var request = new EditContactDetailsRequest { };
            var requestBody = string.Empty;

            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            // Act
            var result = await _classUnderTest.EditContactDetails(query, request, requestBody).ConfigureAwait(false);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task EditContactDetails_WhenNoChanges_DoesntUpdateDatabase()
        {
            // Arrange
            var contactInformation = _fixture.Create<ContactInformation>();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);

            var request = new EditContactDetailsRequest
            {
                ContactInformation = contactInformation
            };
            var requestBody = string.Empty;

            var newDescription = _fixture.Create<string>();

            var updaterResponse = new UpdateEntityResult<ContactDetailsEntity>
            {
                NewValues = new Dictionary<string, object> { },
                UpdatedEntity = new ContactDetailsEntity
                {
                    Id = entity.Id,
                    TargetId = entity.TargetId,
                    ContactInformation = new ContactInformation
                    {
                        Description = newDescription
                    }

                }
            };

            _updater
                .Setup(x => x.UpdateEntity(It.IsAny<ContactDetailsEntity>(), It.IsAny<string>(), It.IsAny<EditContactDetailsDatabase>()))
                .Returns(updaterResponse);

            var query = new EditContactDetailsQuery
            {
                PersonId = entity.TargetId,
                ContactDetailId = entity.Id
            };

            // Act
            var result = await _classUnderTest.EditContactDetails(query, request, requestBody).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            var databaseResponse = await _dbFixture.DynamoDbContext.LoadAsync<ContactDetailsEntity>(entity.TargetId, entity.Id).ConfigureAwait(false);
            databaseResponse.ContactInformation.Description.Should().NotBe(newDescription);
        }

        [Fact]
        public async Task EditContactDetails_WhenChanges_SavesChangesToDatabase()
        {
            // Arrange
            var contactInformation = _fixture.Create<ContactInformation>();

            var entity = _fixture.Build<ContactDetailsEntity>()
                .With(x => x.ContactInformation, contactInformation)
                .With(x => x.RecordValidUntil, DateTime.UtcNow)
                .With(x => x.IsActive, true)
                .With(x => x.LastModified, DateTime.UtcNow)
                .Create();

            await InsertDataIntoDynamoDB(entity).ConfigureAwait(false);


            var request = new EditContactDetailsRequest
            {
                ContactInformation = contactInformation
            };

            var requestBody = string.Empty;

            var newDescription = "Some new description";
            entity.ContactInformation.Description = newDescription;

            var updaterResponse = new UpdateEntityResult<ContactDetailsEntity>
            {
                NewValues = new Dictionary<string, object>
                {
                    { "Description", newDescription }
                },
                UpdatedEntity = entity
            };

            _updater
                .Setup(x => x.UpdateEntity(It.IsAny<ContactDetailsEntity>(), It.IsAny<string>(), It.IsAny<EditContactDetailsDatabase>()))
                .Returns(updaterResponse);

            var query = new EditContactDetailsQuery
            {
                PersonId = entity.TargetId,
                ContactDetailId = entity.Id
            };

            // Act
            var result = await _classUnderTest.EditContactDetails(query, request, requestBody).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();

            var databaseResponse = await _dbFixture.DynamoDbContext.LoadAsync<ContactDetailsEntity>(entity.TargetId, entity.Id).ConfigureAwait(false);

            databaseResponse.ContactInformation.Description.Should().Be(newDescription);
            _cleanup.Add(async () => await _dbFixture.DynamoDbContext.DeleteAsync(entity).ConfigureAwait(false));
        }
    }
}
