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

        public async Task<List<ContactByUprn>> ExecuteAsync()
        {
            // 1. Scan all assets
            var assets = await _gateway.FetchAllAssets();

            // remove assets without a uprn
            var filteredAssets = assets
                .Where(x => !string.IsNullOrWhiteSpace(x.Uprn))
                .ToList();

            // 2. Fetch all contact details
            var contactDetails = await _gateway.FetchAllContactDetails();


            var contactDetailsGroupedByTargetId = contactDetails
                .GroupBy(c => c.TargetId)
                .ToDictionary(group => group.Key, group => group.ToList());

            // 2. Fetch tenure records
            var filterTenures = assets.Where(x => !string.IsNullOrWhiteSpace(x.TenureId.ToString())).ToList();
            var tenureIds = filterTenures.Select(x => x.TenureId).Distinct().ToList();
            var tenures = await _gateway.FetchTenures(tenureIds);
            var tenuresByTenureId = tenures.ToDictionary(x => x.Id, x => x);

            // 3. For each household member, fetch contact details, and person recond
            var personIds = tenures.SelectMany(tenure => tenure.HouseholdMembers.Select(x => x.Id)).Distinct().ToList();
            var persons = await _gateway.FetchPersons(personIds);
            var personById = persons.ToDictionary(x => x.Id, x => x);

            // 4. consolidate the data
            var contacts = _gateway.GetContactByUprnForEachAsset(assets, tenuresByTenureId, personById, contactDetailsGroupedByTargetId);

            return contacts;
        }
    }
}
