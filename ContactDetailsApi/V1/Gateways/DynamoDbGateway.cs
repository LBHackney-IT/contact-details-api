using Amazon.DynamoDBv2.DataModel;
using Amazon.XRay.Recorder.Core.Sampling;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V1.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public async Task<List<ContactDetails>> GetContactByTargetId(Guid targetId)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for targetId parameter {targetId}");
            List<ContactDetailsEntity> contactDetailsEntities = new List<ContactDetailsEntity>();
            var queryResult = _dynamoDbContext.QueryAsync<ContactDetailsEntity>(targetId, null);
            if (queryResult == null)
            {
                return new List<ContactDetails>();
            }
            while (!queryResult.IsDone)
            {
                contactDetailsEntities.AddRange(await queryResult.GetNextSetAsync().ConfigureAwait(false));
            }
            return contactDetailsEntities?.ToDomain();
        }
    }
}
