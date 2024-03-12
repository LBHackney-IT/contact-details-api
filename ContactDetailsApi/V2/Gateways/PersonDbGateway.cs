using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V2.Gateways.Interfaces;
using Hackney.Shared.Person.Infrastructure;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;

namespace ContactDetailsApi.V2.Gateways
{
    public class PersonDbGateway : IPersonDbGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<PersonDbGateway> _logger;

        public PersonDbGateway(IDynamoDBContext dynamoDbContext, ILogger<PersonDbGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
        }

        [LogCall]
        public async Task<List<PersonDbEntity>> GetPersons(List<Guid> ids)
        {
            _logger.LogInformation($"Calling IDynamoDBContext.BatchGetAsync for persons with ids [{string.Join(", ", ids)}]");
            var batchGet = _dynamoDbContext.CreateBatchGet<PersonDbEntity>();
            foreach (var id in ids)
            {
                batchGet.AddKey(id);
            }

            await batchGet.ExecuteAsync().ConfigureAwait(false);
            return batchGet.Results;
        }
    }
}
