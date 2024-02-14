using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
{
    public interface IFetchAllContactDetailsByUprnUseCase
    {
        Task<List<ContactByUprn>> ExecuteAsync();
    }

    public class FetchAllContactDetailsByUprnUseCase : IFetchAllContactDetailsByUprnUseCase
    {

        private readonly IContactDetailsGateway _gateway;

        public FetchAllContactDetailsByUprnUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<List<ContactByUprn>> ExecuteAsync()
        {
            var results = await _gateway.FetchAllContactDetailsByUprnUseCase();

            return results;
        }
    }
}
