using Hackney.Shared.Tenure.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface ITenureDbGateway
    {

        public Task<IEnumerable<TenureInformation>> GetAllTenures();
    }
}
