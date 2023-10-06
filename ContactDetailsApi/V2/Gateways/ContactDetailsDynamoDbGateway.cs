using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Exceptions;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using ContactDetailsApi.V2.UseCase;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{

    public class EditContactDetailsDatabase
    {
        public ContactInformation ContactInformation { get; set; }
        public DateTime? LastModified { get; set; }
    }

    public class ContactDetailsDynamoDbGateway : IContactDetailsGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<ContactDetailsDynamoDbGateway> _logger;
        private readonly IEntityUpdater _updater;

        public ContactDetailsDynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<ContactDetailsDynamoDbGateway> logger, IEntityUpdater updater)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
            _updater = updater;
        }

        [LogCall]
        public async Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for targetId {query.TargetId.Value}");

            List<ContactDetailsEntity> contactDetailsEntities = new List<ContactDetailsEntity>();
            DynamoDBOperationConfig dbOperationConfig = null;

            if (!query.IncludeHistoric)
            {
                dbOperationConfig = CreateConfigForOnlyActiveContactDetails();
            }

            var search = _dynamoDbContext.QueryAsync<ContactDetailsEntity>(query.TargetId.Value, dbOperationConfig);

            do
            {
                var newResults = await search.GetNextSetAsync().ConfigureAwait(false);

                contactDetailsEntities.AddRange(newResults);
            } while (!search.IsDone);

            return contactDetailsEntities.ToDomain();
        }

        [LogCall]
        public async Task<EditContactDetailsDomain> EditContactDetails(Guid contactDetailsId, EditContactDetailsRequest request, string requestBody, int? ifMatch)
        {
            _logger.LogDebug("Calling IDynamoDBContext.LoadAsync for {ContactDetailsId}", contactDetailsId);

            var existingContactDetails = await _dynamoDbContext.LoadAsync<ContactDetailsEntity>(contactDetailsId).ConfigureAwait(false);
            if (existingContactDetails == null) return null;

            if (ifMatch != existingContactDetails.VersionNumber)
                throw new VersionNumberConflictException(ifMatch, existingContactDetails.VersionNumber);

            var updaterResponse = _updater.UpdateEntity<ContactDetailsEntity, EditContactDetailsDatabase>(
                existingContactDetails,
                requestBody,
                request.ToDatabase()
            );


            if (updaterResponse.NewValues.Any())
            {
                _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync to update id {contactDetailsId}");
                await _dynamoDbContext.SaveAsync<ContactDetailsEntity>(updaterResponse.UpdatedEntity).ConfigureAwait(false);
            }

            return new EditContactDetailsDomain
            {
                UpdateResult = updaterResponse,
                ExistingEntity = existingContactDetails
            };
        }

        private static DynamoDBOperationConfig CreateConfigForOnlyActiveContactDetails()
        {
            var onlyActiveCondition = new ScanCondition(nameof(ContactDetailsEntity.IsActive), ScanOperator.Equal, true);

            var scanConditions = new List<ScanCondition>
            {
                onlyActiveCondition
            };

            return new DynamoDBOperationConfig { QueryFilter = scanConditions };
        }

        [LogCall]
        public async Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for targetId {contactDetails.TargetId} and id {contactDetails.Id}");

            contactDetails.LastModified = DateTime.UtcNow;
            await _dynamoDbContext.SaveAsync(contactDetails).ConfigureAwait(false);

            return contactDetails.ToDomain();
        }
    }
}