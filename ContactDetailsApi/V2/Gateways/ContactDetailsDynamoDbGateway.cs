using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{
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
                var newResults = await search.GetRemainingAsync().ConfigureAwait(false);

                contactDetailsEntities.AddRange(newResults);
            } while (!search.IsDone);

            ContactDetails SafeToDomain(ContactDetailsEntity cdEntity)
            {
                try
                {
                    return cdEntity?.ToDomain();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error: Failed to convert contact details {CdEntity} to ContactDetails domain", cdEntity?.Id);
                    return null;
                };
            };

            return contactDetailsEntities?.Select(cdEntity => SafeToDomain(cdEntity)).Where(cdEnity => cdEnity != null).ToList();
        }

        [LogCall]
        public async Task<UpdateEntityResult<ContactDetailsEntity>> EditContactDetails(EditContactDetailsQuery query, EditContactDetailsRequest request, string requestBody)
        {
            _logger.LogDebug("Calling IDynamoDBContext.LoadAsync for {ContactId} {PersonId}", query.ContactDetailId, query.PersonId);

            var existingContactDetails = await _dynamoDbContext.LoadAsync<ContactDetailsEntity>(query.PersonId, query.ContactDetailId).ConfigureAwait(false);
            if (existingContactDetails == null) return null;

            var updaterResponse = _updater.UpdateEntity(
                existingContactDetails,
                requestBody,
                request.ToDatabase()
            );

            if (updaterResponse.NewValues.Any())
            {
                _logger.LogDebug("Calling IDynamoDBContext.SaveAsync to update  with contactId:{ContactId} and personId:{PersonId}", query.ContactDetailId, query.PersonId);

                updaterResponse.UpdatedEntity.LastModified = DateTime.UtcNow;
                await _dynamoDbContext.SaveAsync(updaterResponse.UpdatedEntity).ConfigureAwait(false);
            }

            return updaterResponse;
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

        [LogCall]
        public async Task<Dictionary<Guid, IEnumerable<ContactDetails>>> GetContactDetailsByTargetIds(IEnumerable<Guid> targetIds)
        {
            var contactDetails = new Dictionary<Guid, IEnumerable<ContactDetails>>();
            foreach (var targetId in targetIds)
            {
                var data = await GetContactDetailsByTargetId(new ContactQueryParameter { TargetId = targetId })
                    .ConfigureAwait(false);
                contactDetails.Add(targetId, data);
            }

            return contactDetails;
        }

        [LogCall]
        public async Task<Dictionary<Guid, IEnumerable<ContactDetails>>> BatchGetContactDetailsByTargetId(List<Guid> targetIds)
        {
            var tasks = new List<Task<Dictionary<Guid, IEnumerable<ContactDetails>>>>();
            var batchSize = 100;

            int numberOfBatches = (int) Math.Ceiling((double) targetIds.Count / batchSize);
            _logger.LogDebug($"Batching contact details for {targetIds.Count} persons in {numberOfBatches} batches of {batchSize} each.");

            for (int i = 0; i < numberOfBatches; i++)
            {
                var currentIds = targetIds.Skip(i * batchSize).Take(batchSize);
                tasks.Add(GetContactDetailsByTargetIds(currentIds));
            }

            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            return results.SelectMany(dict => dict)
                          .ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}
