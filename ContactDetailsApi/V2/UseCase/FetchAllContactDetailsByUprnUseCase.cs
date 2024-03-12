using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Infrastructure;
using Hackney.Core.Logging;

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

        [LogCall]
        public async Task<List<ContactByUprn>> ExecuteAsync()
        {
            var tenures = await _tenureGateway.GetAllTenures().ConfigureAwait(false);

            var personIds = tenures.Select(x => x.HouseholdMembers.Select(y => y.Id))
                                   .SelectMany(x => x)
                                   .Distinct()
                                   .ToList();

            var personsData = await _personGateway.GetPersons(personIds).ConfigureAwait(false);
            var persons = personsData.ToDictionary(x => x.Id, x => x);

            var contactDetails = new Dictionary<Guid, List<ContactDetails>>();
            foreach (var personId in personIds)
            {
                var data = await _contactGateway
                    .GetContactDetailsByTargetId(new ContactQueryParameter { TargetId = personId })
                    .ConfigureAwait(false);
                contactDetails.Add(personId, data);
            }
            return ConsolidateData(tenures, persons, contactDetails);
        }

        [LogCall]
        public List<ContactByUprn> ConsolidateData(List<TenureInformationDb> tenures, Dictionary<Guid, PersonDbEntity> persons, Dictionary<Guid, List<ContactDetails>> contactDetails)
        {
            var contactsByUprn = new List<ContactByUprn>();

            foreach (var tenure in tenures)
            {
                var contacts = new List<Person>();

                // Loop through each household member and create a Person object for each one
                tenure.HouseholdMembers.ForEach(householdMember =>
                {
                    if (!persons.TryGetValue(householdMember.Id, out var personDetails))
                        return; // Skip if the person details are not found

                    var personContactDetails = new List<PersonContactDetails>();
                    if (contactDetails.TryGetValue(householdMember.Id, out var rawContactDetails))
                    {
                        rawContactDetails.ForEach(x =>
                            personContactDetails.Add(new PersonContactDetails
                            {
                                ContactType = x.ContactInformation.ContactType,
                                SubType = x.ContactInformation.SubType,
                                Value = x.ContactInformation.Value
                            }));
                    } // Add personContactDetails if contactDetails are found

                    contacts.Add(new Person()
                    {
                        PersonTenureType = householdMember.PersonTenureType,
                        IsResponsible = householdMember.IsResponsible,
                        FirstName = personDetails.FirstName,
                        LastName = personDetails.Surname,
                        Title = personDetails.Title,
                        PersonContactDetails = personContactDetails
                    }); // Add the person to the contacts list
                });

                contactsByUprn.Add(new ContactByUprn
                {
                    TenureId = tenure.Id,
                    Uprn = tenure.TenuredAsset.Uprn,
                    Contacts = contacts
                }); // Add the contacts to the contactsByUprn list
            }

            return contactsByUprn;
        }
    }
}
