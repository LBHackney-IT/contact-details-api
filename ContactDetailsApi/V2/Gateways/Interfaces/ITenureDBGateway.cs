using Hackney.Shared.Tenure.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface ITenureDbGateway
    {

        public Task<IEnumerable<TenureInformationDb>> GetAllTenures();
    }
}
