using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways.Interfaces;
using ContactDetailsApi.V1.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Gateways
{
    public class ContactDetailsDynamoDbGateway : IContactDetailsGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<ContactDetailsDynamoDbGateway> _logger;

        public ContactDetailsDynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<ContactDetailsDynamoDbGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
        }

        [LogCall]
        public async Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for targetId {query.TargetId.Value}");

            List<ContactDetailsEntity> contactDetailsEntities = new List<ContactDetailsEntity>();
            DynamoDBOperationConfig dbOperationConfig = null;
            if (!query.IncludeHistoric)
            {
                List<ScanCondition> scanConditions = new List<ScanCondition>
                {
                    new ScanCondition(nameof(ContactDetailsEntity.IsActive), ScanOperator.Equal, true)
                };
                dbOperationConfig = new DynamoDBOperationConfig() { QueryFilter = scanConditions };
            }

            var queryResult = _dynamoDbContext.QueryAsync<ContactDetailsEntity>(query.TargetId.Value, dbOperationConfig);
            while (!queryResult.IsDone)
                contactDetailsEntities.AddRange(await queryResult.GetNextSetAsync().ConfigureAwait(false));

            return contactDetailsEntities.ToDomain();
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
        public async Task<ContactDetails> DeleteContactDetailsById(DeleteContactQueryParameter query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.LoadAsync for targetId {query.TargetId} and id {query.Id}");

            var entity = await _dynamoDbContext.LoadAsync<ContactDetailsEntity>(query.TargetId, query.Id).ConfigureAwait(false);
            if (entity == null) return null;
            entity.IsActive = false;
            entity.LastModified = DateTime.UtcNow;

            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for targetId {query.TargetId} and id {query.Id}");
            await _dynamoDbContext.SaveAsync<ContactDetailsEntity>(entity).ConfigureAwait(false);

            return entity.ToDomain();
        }
    }
}
