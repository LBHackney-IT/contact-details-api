using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ContactDetailsApi.V2.Gateways.Interfaces;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;
using Hackney.Shared.Tenure.Domain;
using System.Linq;
using Hackney.Shared.Tenure.Factories;

namespace ContactDetailsApi.V2.Gateways
{
    public class TenureDbGateway : ITenureDbGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<TenureDbGateway> _logger;

        public TenureDbGateway(IDynamoDBContext dynamoDbContext, ILogger<TenureDbGateway> logger)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
        }

        [LogCall]
        public async Task<IEnumerable<TenureInformation>> GetAllTenures()
        {

            var search = _dynamoDbContext.FromScanAsync<TenureInformationDb>(new ScanOperationConfig());

            _logger.LogInformation("Calling IDynamoDBContext.ScanAsync for all tenures");

            var results = await search.GetNextSetAsync().ConfigureAwait(false);
            return results.Select(x => x.ToDomain());
        }
    }
}
