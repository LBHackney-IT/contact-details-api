using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Gateways
{
    public class DynamoDbGateway : IContactDetailsGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;

        public DynamoDbGateway(IDynamoDBContext dynamoDbContext)
        {
            _dynamoDbContext = dynamoDbContext;
        }

        public async Task<List<ContactDetails>> GetContactByTargetId(Guid targetId)
        {
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
