using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Gateways
{
    public class DynamoDbGateway : IContactDetailsGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<DynamoDbGateway> _logger;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<DynamoDbGateway> logger)
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
        public async Task<ContactDetails> DeleteContactDetailsByTargetId(ContactQueryParameter query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for targetId {query.TargetId} and id {query.Id}");
            DynamoDBOperationConfig dbOperationConfig = null;

            List<ScanCondition> scanConditions = new List<ScanCondition>
                {
                    new ScanCondition(nameof(ContactDetailsEntity.Id), ScanOperator.Equal, query.Id),
                    new ScanCondition(nameof(ContactDetailsEntity.TargetId), ScanOperator.Equal, query.TargetId)
                };

            dbOperationConfig = new DynamoDBOperationConfig() { QueryFilter = scanConditions };
            var result = _dynamoDbContext.QueryAsync<ContactDetailsEntity>(query.TargetId.Value, dbOperationConfig);
            ContactDetailsEntity entity = null;
            while (!result.IsDone)
            {
                entity = result.GetNextSetAsync().Result.First();
                entity.IsActive = false;
                await _dynamoDbContext.SaveAsync<ContactDetailsEntity>(entity).ConfigureAwait(false);
                break;
            }
            if (entity == null) throw new NullReferenceException($"Contact with target id {query.TargetId} and id {query.Id} not found");

            return entity.ToDomain();
        }



    }

}
