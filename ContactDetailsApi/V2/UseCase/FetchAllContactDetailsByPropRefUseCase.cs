using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using Hackney.Shared.Tenure.Domain;
using Hackney.Core.Logging;
using ContactDetailsApi.V2.Boundary.Response;
using Hackney.Core.DynamoDb;

namespace ContactDetailsApi.V2.UseCase
{
    public class FetchAllContactDetailsByPropRefUseCase : IFetchAllContactDetailsByUprnUseCase
    {

        private readonly ITenureDbGateway _tenureGateway;
        private readonly IPersonDbGateway _personGateway;
        private readonly IContactDetailsGateway _contactGateway;

        public FetchAllContactDetailsByPropRefUseCase(ITenureDbGateway tenureGateway, IPersonDbGateway personGateway, IContactDetailsGateway contactGateway)
        {
            _tenureGateway = tenureGateway;
            _personGateway = personGateway;
            _contactGateway = contactGateway;
        }

        private async Task<PagedResult<TenureInformation>> GetTenures(string paginationToken, int pageSize)
        {
            var tenures = await _tenureGateway.ScanTenures(paginationToken, pageSize).ConfigureAwait(false);
            tenures.Results = tenures.Results.Where(x => x.TenuredAsset?.PropertyReference != null && x.IsActive == true)
                            .GroupBy(x => x.TenuredAsset.PropertyReference)
                            .Select(x => x.FirstOrDefault())
                             .ToList();
            return tenures;
        }

        private async Task<Dictionary<Guid, Hackney.Shared.Person.Person>> GetPersons(List<Guid> personIds)
        {
            var personsData = await _personGateway.GetPersons(personIds).ConfigureAwait(false);
            var persons = personsData.ToDictionary(x => x.Id, x => x);
            return persons;
        }

        private async Task<Dictionary<Guid, IEnumerable<ContactDetails>>> GetContactDetails(List<Guid> targetIds)
        {
            return await _contactGateway.BatchGetContactDetailsByTargetId(targetIds).ConfigureAwait(false);
        }

        private static List<Guid> FilterPersonIds(IEnumerable<TenureInformation> tenures)
        {
            var personIds = tenures
                .Select(x => x.HouseholdMembers.Where(x => x.IsResponsible)
                    .Select(y => y.Id))
                .SelectMany(x => x)
                .Distinct()
                .ToList();

            return personIds;
        }

        private static List<PersonContactDetails> PersonContactDetailsList(Dictionary<Guid, IEnumerable<ContactDetails>> contactDetails, HouseholdMembers householdMember)
        {
            if (contactDetails.TryGetValue(householdMember.Id, out var rawContactDetails))
            {
                return rawContactDetails.ToContactByPropRefPersonContacts();
            }
            return new List<PersonContactDetails>();
            // Add personContactDetails if contactDetails are found, else empty list
        }

        private static List<Person> GetTenureContacts(Dictionary<Guid, Hackney.Shared.Person.Person> persons, Dictionary<Guid, IEnumerable<ContactDetails>> contactDetails, TenureInformation tenure)
        {
            var contacts = new List<Person>();

            // Loop through each household member and create a Person object for each one
            foreach (var householdMember in tenure.HouseholdMembers)
            {
                if (!persons.TryGetValue(householdMember.Id, out var personDetails))
                    continue; // Skip if the person details are not found

                var personContactDetails = PersonContactDetailsList(contactDetails, householdMember);
                contacts.Add(personDetails.ToContactByPropRefPerson(householdMember, personContactDetails)); // Add the person to the contacts list
            }
            return contacts;
        }

        [LogCall]
        public List<ContactByPropRef> ConsolidateData(IEnumerable<TenureInformation> tenures, Dictionary<Guid, Hackney.Shared.Person.Person> persons, Dictionary<Guid, IEnumerable<ContactDetails>> contactDetails)
        {
            var contactsByUprn = new List<ContactByPropRef>();

            foreach (var tenure in tenures)
            {
                var contacts = GetTenureContacts(persons, contactDetails, tenure);
                contactsByUprn.Add(tenure.ToContactByPropRef(contacts));
            }

            return contactsByUprn;
        }

        [LogCall]
        public async Task<PagedResult<ContactByPropRef>> ExecuteAsync(ServicesoftFetchContactDetailsRequest request)
        {
            var tenures = await GetTenures(request.PaginationToken, request.PageSize).ConfigureAwait(false);
            var personIds = FilterPersonIds(tenures.Results);

            var persons = await GetPersons(personIds);
            var contactDetails = await GetContactDetails(personIds);

            var data = ConsolidateData(tenures.Results, persons, contactDetails);
            return new PagedResult<ContactByPropRef>(data, tenures.PaginationDetails);
        }
    }
}
