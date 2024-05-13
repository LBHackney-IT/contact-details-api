using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using ContactDetailsApi.V2.Gateways.Interfaces;
using Hackney.Shared.Person.Infrastructure;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;
using System.Linq;
using Hackney.Shared.Person.Factories;
using Hackney.Shared.Person;

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
        public async Task<IEnumerable<Person>> GetPersons(List<Guid> ids)
        {
            _logger.LogInformation($"Calling IDynamoDBContext.BatchGetAsync for {ids.Count} persons");
            var batchGet = _dynamoDbContext.CreateBatchGet<PersonDbEntity>();
            foreach (var id in ids)
            {
                batchGet.AddKey(id);
            }

            Person SafeToDomain(PersonDbEntity person)
            {
                try
                {
                    return person?.ToDomain();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error: Failed to convert person {PersonDb} to Person domain", person?.Id);
                    return null;
                };
            };

            await batchGet.ExecuteAsync().ConfigureAwait(false);
            return batchGet.Results.Select(person => SafeToDomain(person)).Where(person => person != null);
        }
    }
}
