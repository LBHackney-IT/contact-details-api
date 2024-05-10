using Hackney.Shared.Tenure.Domain;
using Hackney.Core.DynamoDb;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface ITenureDbGateway
    {

        public Task<PagedResult<TenureInformation>> ScanTenures(string paginationToken, int? pageSize);
    }
}
