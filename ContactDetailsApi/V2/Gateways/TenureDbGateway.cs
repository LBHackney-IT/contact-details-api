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
using Newtonsoft.Json;


namespace ContactDetailsApi.V2.Gateways;

public class TenureDbGateway: ITenureDbGateway
{
    private readonly IDynamoDBContext _dynamoDbContext;
    private readonly ILogger<TenureDbGateway> _logger;

    public TenureDbGateway(IDynamoDBContext dynamoDbContext, ILogger<TenureDbGateway> logger)
    {
        _dynamoDbContext = dynamoDbContext;
        _logger = logger;
    }

    [LogCall]
    public async Task<Tuple<List<TenureInformation>, Guid?>> ScanTenures(Guid? lastEvaluatedKey)
    {
        var scanConfig = new ScanOperationConfig {
                Limit = 10
            };
        if(lastEvaluatedKey.HasValue)
            scanConfig.PaginationToken = lastEvaluatedKey.ToString();

        var search =  _dynamoDbContext.FromScanAsync<TenureInformationDb>(scanConfig);
        _logger.LogInformation("Calling IDynamoDBContext.ScanAsync for all tenures");

        var tenures = await search.GetNextSetAsync().ConfigureAwait(false);

        var paginationToken = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(search.PaginationToken);
        Guid? newLastKey = search.PaginationToken != null ? Guid.Parse(paginationToken["id"]["S"]) : null;
        var responseData = tenures.Select(x => x.ToDomain()).ToList();

        return Tuple.Create(responseData, newLastKey);
    }
}
