using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
{
    public class FetchAllContactDetailsByUprnUseCase : IFetchAllContactDetailsByUprnUseCase
    {

        private readonly IContactDetailsGateway _gateway;

        public FetchAllContactDetailsByUprnUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<ContactByUprn>> ExecuteAsync(FetchAllContactDetailsQuery query)
        {
            var results = await _gateway.FetchAllContactDetailsByUprnUseCase(query);

            return results;
        }
    }
}
