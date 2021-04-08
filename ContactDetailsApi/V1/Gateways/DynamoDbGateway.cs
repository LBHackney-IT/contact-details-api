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

        public async Task<ContactDetails> GetEntityById(Guid id)
        {
            var result = await _dynamoDbContext.LoadAsync<ContactDetailsEntity>(id).ConfigureAwait(false);
            return result?.ToDomain();
        }
    }
}
