using System;
using System.Collections.Generic;
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
            ConsistentRead = true,
            Limit = pageSize ?? int.MaxValue,
            PaginationToken = PaginationDetails.DecodeToken(paginationToken)
        };

        var search = _dynamoDbContext.FromScanAsync<TenureInformationDb>(scanConfig);
        _logger.LogInformation("Calling IDynamoDBContext.ScanAsync for TenureInformationDb");

        var resultsSet = await search.GetNextSetAsync().ConfigureAwait(false);
        paginationToken = search.PaginationToken;

        _logger.LogInformation("Returned {resultsCount} results from IDynamoDBContext.ScanAsync " +
                               "for TenureInformationDb with pagination token {paginationToken}",
            resultsSet.Count, paginationToken);

        return new PagedResult<TenureInformation>(
            resultsSet.Select(x => x.ToDomain()),
            new PaginationDetails(paginationToken)
        );
    }
}

