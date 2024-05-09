using System;
using Hackney.Shared.Tenure.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface ITenureDbGateway
    {

        public Task<Tuple<List<TenureInformation>, Guid?>> ScanTenures(Guid? lastEvaluatedKey);
    }
}
