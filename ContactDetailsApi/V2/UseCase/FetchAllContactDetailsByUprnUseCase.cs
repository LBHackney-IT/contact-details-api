using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using Hackney.Shared.Tenure.Domain;
using Hackney.Core.Logging;
using ContactDetailsApi.V2.Boundary.Response;

namespace ContactDetailsApi.V2.UseCase
{
    public class FetchAllContactDetailsByUprnUseCase : IFetchAllContactDetailsByUprnUseCase
    {

        private readonly ITenureDbGateway _tenureGateway;
        private readonly IPersonDbGateway _personGateway;
        private readonly IContactDetailsGateway _contactGateway;

        public FetchAllContactDetailsByUprnUseCase(ITenureDbGateway tenureGateway, IPersonDbGateway personGateway, IContactDetailsGateway contactGateway)
        {
            _tenureGateway = tenureGateway;
            _personGateway = personGateway;
            _contactGateway = contactGateway;
        }

        private async Task<IEnumerable<TenureInformation>> GetTenures()
        {
            var tenures = await _tenureGateway.GetAllTenures().ConfigureAwait(false);
            tenures = tenures.Where(x => x.IsActive)
                          .GroupBy(x => x.TenuredAsset.Uprn) // Group by UPRN to get tenures per property
                          .Select(x => x.Where(x => x.TenuredAsset?.Uprn != null)) // filter out tenures with no UPRN
                          .Select(x => x.FirstOrDefault())// Get the active tenure for each property
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
            var personIds = tenures.Select(x => x.HouseholdMembers.Where(x => x.IsResponsible)
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
                return rawContactDetails.ToContactByUprnPersonContacts();
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
                contacts.Add(personDetails.ToContactByUprnPerson(householdMember, personContactDetails)); // Add the person to the contacts list
            }
            return contacts;
        }

        [LogCall]
        public List<ContactByUprn> ConsolidateData(IEnumerable<TenureInformation> tenures, Dictionary<Guid, Hackney.Shared.Person.Person> persons, Dictionary<Guid, IEnumerable<ContactDetails>> contactDetails)
        {
            var contactsByUprn = new List<ContactByUprn>();

            foreach (var tenure in tenures)
            {
                var contacts = GetTenureContacts(persons, contactDetails, tenure);
                contactsByUprn.Add(tenure.ToContactByUprn(contacts));
            }

            return contactsByUprn;
        }

        [LogCall]
        public async Task<ContactsByUprnList> ExecuteAsync()
        {
            var tenures = await GetTenures().ConfigureAwait(false);
            var personIds = FilterPersonIds(tenures.ToList());
            if (personIds == null) return null;
            var persons = await GetPersons(personIds);
            var contactDetails = await GetContactDetails(personIds);

            var data = ConsolidateData(tenures, persons, contactDetails);
            return data?.ToResponse();
        }
    }
}
