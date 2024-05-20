using System;
using System.Threading.Tasks;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;
using Hackney.Shared.Tenure.Domain;
using System.Linq;
using Amazon.DynamoDBv2.DocumentModel;
using ContactDetailsApi.V2.Gateways.Interfaces;
using Hackney.Shared.Tenure.Factories;
using Amazon.DynamoDBv2.DataModel;
using Hackney.Core.DynamoDb;

namespace ContactDetailsApi.V2.Gateways;

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
    public async Task<PagedResult<TenureInformation>> ScanTenures(string paginationToken, int? pageSize)
    {
        var scanConfig = new ScanOperationConfig
        {
            ConsistentRead = false,
            Limit = pageSize ?? Int32.MaxValue,
            PaginationToken = PaginationDetails.DecodeToken(paginationToken)
        };

        var search = _dynamoDbContext.FromScanAsync<TenureInformationDb>(scanConfig);
        _logger.LogInformation("Calling IDynamoDBContext.ScanAsync for TenureInformationDb");

        var resultsSet = await search.GetNextSetAsync().ConfigureAwait(false);
        paginationToken = search.PaginationToken;

        TenureInformation SafeToDomain(TenureInformationDb tenure)
        {
            try
            {
                return tenure?.ToDomain();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error: Failed to convert tenure {Tenure} to TenureInformation domain", tenure?.Id);
                return null;
            };
        };

        return new PagedResult<TenureInformation>(
            resultsSet.Select(tenure => SafeToDomain(tenure))
            .Where(tenure => tenure != null),
            new PaginationDetails(paginationToken)
        );
    }
}

