using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Infrastructure;
using Hackney.Core.Logging;
using Hackney.Shared.Tenure.Domain;

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

        private async Task<IEnumerable<TenureInformationDb>> GetTenures()
        {
            var tenures = await _tenureGateway.GetAllTenures().ConfigureAwait(false);
            tenures = tenures.Where(x => x.TenuredAsset?.Uprn != null).ToList();
            return tenures;
        }

        private async Task<Dictionary<Guid, PersonDbEntity>> GetPersons(List<Guid> personIds)
        {
            var personsData = await _personGateway.GetPersons(personIds).ConfigureAwait(false);
            var persons = personsData.ToDictionary(x => x.Id, x => x);
            return persons;
        }

        private async Task<Dictionary<Guid, List<ContactDetails>>> GetContactDetails(List<Guid> personIds)
        {
            var contactDetails = new Dictionary<Guid, List<ContactDetails>>();

            var tasks = personIds.Select(async personId =>
            {
                var data = await _contactGateway
                    .GetContactDetailsByTargetId(new ContactQueryParameter { TargetId = personId })
                    .ConfigureAwait(false);
                contactDetails.Add(personId, data);
            });

            await Task.WhenAll(tasks).ConfigureAwait(false);
            return contactDetails;
        }

        private static List<Guid> FilterPersonIds(IEnumerable<TenureInformationDb> tenures)
        {
            var personIds = tenures.Select(x => x.HouseholdMembers.Select(y => y.Id))
                .SelectMany(x => x)
                .Distinct()
                .ToList();

            // TODO: Add filter here to select 1 person per tenure (will be in a future PR)
            return personIds;
        }

        [LogCall]
        public async Task<List<ContactByUprn>> ExecuteAsync()
        {
            var tenures = await GetTenures().ConfigureAwait(false);
            var personIds = FilterPersonIds(tenures.ToList());

            var persons = await GetPersons(personIds);
            var contactDetails = await GetContactDetails(personIds);

            return ConsolidateData(tenures, persons, contactDetails);
        }

        private static List<PersonContactDetails> PersonContactDetailsList(Dictionary<Guid, List<ContactDetails>> contactDetails, HouseholdMembers householdMember)
        {
            if (contactDetails.TryGetValue(householdMember.Id, out var rawContactDetails))
            {
                return rawContactDetails.ToContactByUprnPersonContacts();
            }
            return new List<PersonContactDetails>();
            // Add personContactDetails if contactDetails are found, else empty list
        }

        private static List<Person> GetTenureContacts(Dictionary<Guid, PersonDbEntity> persons, Dictionary<Guid, List<ContactDetails>> contactDetails, TenureInformationDb tenure)
        {
            var contacts = new List<Person>();

            // Loop through each household member and create a Person object for each one
            tenure.HouseholdMembers.ForEach(householdMember =>
            {
                if (!persons.TryGetValue(householdMember.Id, out var personDetails))
                    return; // Skip if the person details are not found

                var personContactDetails = PersonContactDetailsList(contactDetails, householdMember);
                contacts.Add(personDetails.ToContactByUprnPerson(householdMember, personContactDetails)); // Add the person to the contacts list
            });
            return contacts;
        }

        [LogCall]
        public List<ContactByUprn> ConsolidateData(IEnumerable<TenureInformationDb> tenures, Dictionary<Guid, PersonDbEntity> persons, Dictionary<Guid, List<ContactDetails>> contactDetails)
        {
            var contactsByUprn = new List<ContactByUprn>();

            foreach (var tenure in tenures)
            {
                var contacts = GetTenureContacts(persons, contactDetails, tenure);
                contactsByUprn.Add(tenure.ToContactByUprn(contacts));
            }

            return contactsByUprn;
        }
    }
}
