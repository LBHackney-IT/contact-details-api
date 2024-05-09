using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Hackney.Core.Logging;
using Hackney.Shared.Tenure.Domain;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ContactDetailsApi.V2.Gateways.Interfaces;
using Hackney.Shared.Tenure.Factories;
using Newtonsoft.Json;


namespace ContactDetailsApi.V2.Gateways;

public class TenureDbGateway: ITenureDbGateway
{
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly ILogger<TenureDbGateway> _logger;

    public TenureDbGateway(AmazonDynamoDBClient dynamoDbClient, ILogger<TenureDbGateway> logger)
    {
        _dynamoDbClient = dynamoDbClient;
        _logger = logger;
    }

    [LogCall]
    public async Task<Tuple<List<TenureInformation>, Guid>> ScanTenures(Guid? lastEvaluatedKey)
    {
        Dictionary<string, AttributeValue> exclusiveStartKey = null;

        if (lastEvaluatedKey != null)
            exclusiveStartKey = new Dictionary<string, AttributeValue> {
                {
                    "id", new AttributeValue { S = lastEvaluatedKey.ToString() }
                }
            };

        var scanRequest = new ScanRequest
        {
            TableName = "TenureInformation",
            ExclusiveStartKey = exclusiveStartKey
        };

        var tenures = new List<TenureInformation>();
        var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest).ConfigureAwait(false);

        if (scanResponse.Items.Count > 0)
        {
            tenures = scanResponse.Items
                .Select(item => Document.FromAttributeMap(item).ToJsonPretty())
                .Select(item => JsonConvert.DeserializeObject<TenureInformationDb>(item))
                .Select(item => item.ToDomain())
                .ToList();
        }

        var newKeyString = scanResponse.LastEvaluatedKey.Values.FirstOrDefault().S;
        var newLastKey = Guid.Parse(newKeyString);

        return Tuple.Create(tenures, newLastKey);
    }
}

