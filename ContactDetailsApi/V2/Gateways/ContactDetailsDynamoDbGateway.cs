using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using Hackney.Core.DynamoDb;
using Hackney.Core.Logging;
using Hackney.Shared.Person.Domain;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{
    public class ContactDetailsDynamoDbGateway : IContactDetailsGateway
    {
        private const int MAX_RESULTS = 10;
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<ContactDetailsDynamoDbGateway> _logger;
        private readonly IAmazonDynamoDB _dynamoDB;
        private readonly IEntityUpdater _updater;

        public ContactDetailsDynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<ContactDetailsDynamoDbGateway> logger, IEntityUpdater updater, IAmazonDynamoDB dynamoDB)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
            _updater = updater;
            _dynamoDB = dynamoDB;
        }

        [LogCall]
        public async Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.QueryAsync for targetId {query.TargetId.Value}");

            List<ContactDetailsEntity> contactDetailsEntities = new List<ContactDetailsEntity>();
            DynamoDBOperationConfig dbOperationConfig = null;

            if (!query.IncludeHistoric)
            {
                dbOperationConfig = CreateConfigForOnlyActiveContactDetails();
            }

            var search = _dynamoDbContext.QueryAsync<ContactDetailsEntity>(query.TargetId.Value, dbOperationConfig);

            do
            {
                var newResults = await search.GetRemainingAsync().ConfigureAwait(false);

                contactDetailsEntities.AddRange(newResults);
            } while (!search.IsDone);

            return contactDetailsEntities.ToDomain();
        }

        [LogCall]
        public async Task<UpdateEntityResult<ContactDetailsEntity>> EditContactDetails(EditContactDetailsQuery query, EditContactDetailsRequest request, string requestBody)
        {
            _logger.LogDebug("Calling IDynamoDBContext.LoadAsync for {ContactId} {PersonId}", query.ContactDetailId, query.PersonId);

            var existingContactDetails = await _dynamoDbContext.LoadAsync<ContactDetailsEntity>(query.PersonId, query.ContactDetailId).ConfigureAwait(false);
            if (existingContactDetails == null) return null;

            var updaterResponse = _updater.UpdateEntity(
                existingContactDetails,
                requestBody,
                request.ToDatabase()
            );

            if (updaterResponse.NewValues.Any())
            {
                _logger.LogDebug("Calling IDynamoDBContext.SaveAsync to update  with contactId:{ContactId} and personId:{PersonId}", query.ContactDetailId, query.PersonId);

                updaterResponse.UpdatedEntity.LastModified = DateTime.UtcNow;
                await _dynamoDbContext.SaveAsync(updaterResponse.UpdatedEntity).ConfigureAwait(false);
            }

            return updaterResponse;
        }

        private static DynamoDBOperationConfig CreateConfigForOnlyActiveContactDetails()
        {
            var onlyActiveCondition = new ScanCondition(nameof(ContactDetailsEntity.IsActive), ScanOperator.Equal, true);

            var scanConditions = new List<ScanCondition>
            {
                onlyActiveCondition
            };

            return new DynamoDBOperationConfig { QueryFilter = scanConditions };
        }

        [LogCall]
        public async Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails)
        {
            _logger.LogDebug($"Calling IDynamoDBContext.SaveAsync for targetId {contactDetails.TargetId} and id {contactDetails.Id}");

            contactDetails.LastModified = DateTime.UtcNow;
            await _dynamoDbContext.SaveAsync(contactDetails).ConfigureAwait(false);

            return contactDetails.ToDomain();
        }


        [LogCall(LogLevel.Information)]
        public async Task<List<ContactDetailsEntity>> FetchAllContactDetails(FetchAllContactDetailsQuery query)
        {
            //var rawResults = new List<Document>();
            var pageSize = query.PageSize.HasValue ? query.PageSize.Value : MAX_RESULTS; 
            var table = Table.LoadTable(_dynamoDB, "ContactDetails");
            _logger.LogInformation($"Calling IDynamoDBContext.Scan for Contact details");
            var scan = table.Scan(new ScanOperationConfig
            {
                Limit = pageSize,
                //PaginationToken = PaginationDetails.DecodeToken(query.PaginationToken),
                //ConsistentRead = true
            }) ;

            //do
            //{
            //    var newResults = await scan.GetNextSetAsync();
            //    rawResults.AddRange(newResults);
            //}
            //while (!scan.IsDone);
            var rawResults = await scan.GetNextSetAsync().ConfigureAwait(false);

            var results = new List<ContactDetailsEntity>();

            foreach (var result in rawResults)
            {

                var id = result["id"];
                var targetId = result["targetId"];
                var rawContactInformation = result["contactInformation"].AsDocument();
                var contactValue = rawContactInformation.ContainsKey("value") ? rawContactInformation["value"] : null;
                var contactInformation = new ContactInformation
                {
                    Value = contactValue ?? ""
                };

                var isActive = result["isActive"];


                var entity = new ContactDetailsEntity
                {
                    Id = (Guid) id,
                    TargetId = (Guid) targetId,
                    ContactInformation = contactInformation,
                    IsActive = (bool) isActive
                };

                results.Add(entity);
            }

            return results;
        }

        [LogCall(LogLevel.Information)]
        public async Task<List<TenureInformationDb>> FetchTenures(List<Guid?> tenureIds)
        {
            var table = Table.LoadTable(_dynamoDB, "TenureInformation");

            var tenureBatchRequest = table.CreateBatchGet();
            _logger.LogInformation($"Calling IDynamoDBContext.CreateBatchGet for Tenure Information");

            foreach (Guid id in tenureIds)
            {
                tenureBatchRequest.AddKey(id);
            }

            await tenureBatchRequest.ExecuteAsync();

            var rawResults = tenureBatchRequest.Results;

            var results = new List<TenureInformationDb>();

            foreach (var result in rawResults)
            {
                var id = result["id"];
                var householdMembers = result["householdMembers"];

                var rawHouseholdMembers = householdMembers.AsListOfDocument();
                var householdMemberList = new List<HouseholdMembers>();
                foreach (var member in rawHouseholdMembers)
                {
                    var personId = member.ContainsKey("id") ? member["id"] : null;
                    var isResponsible = member["isResponsible"];
                    var householdMember = new HouseholdMembers
                    {
                        Id = (Guid) personId,
                        IsResponsible = (bool) isResponsible
                    };

                    householdMemberList.Add(householdMember);

                }


                var entity = new TenureInformationDb
                {
                    Id = (Guid) id,
                    HouseholdMembers = householdMemberList
                };

                results.Add(entity);

            }

            return results;
        }

        [LogCall(LogLevel.Information)]
        public async Task<List<PersonDbEntity>> FetchPersons(List<Guid> personIds)
        {
            var table = Table.LoadTable(_dynamoDB, "Persons");
            _logger.LogInformation($"Calling IDynamoDBContext.CreateBatchGet for Persons");

            var personBatchRequest = table.CreateBatchGet();


            foreach (Guid id in personIds)
            {
                personBatchRequest.AddKey(id);
            }

            await personBatchRequest.ExecuteAsync();

            var rawResults = personBatchRequest.Results;

            var results = new List<PersonDbEntity>();
            foreach (var result in rawResults)
            {
                var id = result["id"];
                var firstName = result["firstName"];
                var surname = result["surname"];
                var title = result["title"];
                var entity = new PersonDbEntity
                {
                    Id = (Guid) id,
                    FirstName = firstName,
                    Surname = surname,
                    Title = (Title) Enum.Parse(typeof(Title), title)
                };

                results.Add(entity);
            }

            return results;

        }
        [LogCall(LogLevel.Information)]
        public async Task<List<ContactByUprn>> FetchAllAssets(FetchAllContactDetailsQuery query)
        {
            var pageSize = query.PageSize.HasValue ? query.PageSize.Value : MAX_RESULTS;
            var table = Table.LoadTable(_dynamoDB, "Assets");
            _logger.LogInformation($"Calling IDynamoDBContext.Scan for Assets");
            var scan = table.Scan(new ScanOperationConfig
            {
                Limit = pageSize,
                //PaginationToken = PaginationDetails.DecodeToken(query.PaginationToken),
                //ConsistentRead = true
            });

           
            var rawResults = await scan.GetNextSetAsync().ConfigureAwait(false);

            var results = new List<ContactByUprn>();

            foreach (var result in rawResults)
            {
                var assetAddress = result["assetAddress"].AsDocument();
                var uprn = assetAddress != null && assetAddress.ContainsKey("uprn") ? assetAddress["uprn"] : null;

                var tenure = result.ContainsKey("tenure") ? result["tenure"].AsDocument() : null;
                var tenureId = tenure != null && tenure.ContainsKey("id") ? tenure["id"] : null;
                if (tenureId == null)
                    continue;


                var entity = new ContactByUprn
                {
                    Uprn = uprn?.ToString() ?? "",
                    TenureId = (Guid?) tenureId
                };

                results.Add(entity);

            }

            return results;
        }
        [LogCall(LogLevel.Information)]
        public List<ContactByUprn> GetContactByUprnForEachAsset(List<ContactByUprn> assets,
                                                                 Dictionary<Guid, TenureInformationDb> tenuresByTenureId,
                                                                 Dictionary<Guid, PersonDbEntity> personById,
                                                                 Dictionary<Guid, List<ContactDetailsEntity>> contactDetailsGroupedByTargetId)
        {
            var contacts = new List<ContactByUprn>();
            foreach (var asset in assets)
            {
                if (asset.TenureId == Guid.Empty || asset.TenureId == null)
                    continue;

                Guid tenureId = (Guid) asset.TenureId;

                try
                {
                    var id = tenuresByTenureId[tenureId];
                }
                catch (Exception e)
                {
                    _logger.LogInformation($"tenuresByTenureId for id {tenureId} failed with the following message {e.Message}");
                    continue;
                }

                var tenure = tenuresByTenureId[tenureId];

                var personContacts = new List<Person>();

                foreach (var householdMember in tenure.HouseholdMembers)
                {
                    try
                    {
                        var tenant = personById[householdMember.Id];
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"personById for id {householdMember.Id} failed with the following message {e.Message}");
                        continue;
                    }
                    var person = personById[householdMember.Id];

                    try
                    {
                        var contactDetailsTargetId = contactDetailsGroupedByTargetId[person.Id];
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation($"contactDetailsGroupedByTargetId for id {person.Id} failed with the following message {e.Message}");
                        continue;
                    }

                    var personContactDetails = contactDetailsGroupedByTargetId[person.Id]
                        .Select(x => new PersonContactDetails
                        {
                            ContactType = x.ContactInformation.ContactType.ToString(),
                            SubType = x.ContactInformation.SubType.ToString(),
                            Value = x.ContactInformation.Value.ToString()
                        })
                        .ToList();


                    var personContact = new Person
                    {
                        PersonTenureType = householdMember.PersonTenureType.ToString(),
                        IsResponsible = householdMember.IsResponsible,
                        FirstName = person.FirstName,
                        LastName = person.Surname,
                        Title = person.Title.ToString(),
                        PersonContactDetails = personContactDetails
                    };

                    personContacts.Add(personContact);
                }

                var contactByUprn = new ContactByUprn
                {
                    Uprn = asset.Uprn,
                    TenureId = tenure.Id,
                    Contacts = personContacts
                };

                contacts.Add(contactByUprn);
            }
            return contacts;
        }

        [LogCall(LogLevel.Information)]
        public async Task<List<ContactByUprn>> FetchAllContactDetailsByUprnUseCase(FetchAllContactDetailsQuery query)
        {

            // 1. Scan all assets
            var assets = await FetchAllAssets(query);

            // remove assets without a uprn
            var filteredAssets = assets
                .Where(x => !string.IsNullOrWhiteSpace(x.Uprn))
                .ToList();

            // 2. Fetch all contact details
            var contactDetails = await FetchAllContactDetails(query);

            var contactDetailsGroupedByTargetId = contactDetails
                .GroupBy(c => c.TargetId)
                .ToDictionary(group => group.Key, group => group.ToList());

            // 3. Fetch tenure records
            var filterTenures = assets.Where(x => !string.IsNullOrWhiteSpace(x.TenureId.ToString())).ToList();
            var tenureIds = filterTenures.Select(x => x.TenureId).Distinct().ToList();
            var tenures = await FetchTenures(tenureIds);
            var tenuresByTenureId = tenures.ToDictionary(x => x.Id, x => x);

            // 4. For each household member, fetch contact details, and person recond
            var personIds = tenures.SelectMany(tenure => tenure.HouseholdMembers.Select(x => x.Id)).Distinct().ToList();
            var persons = await FetchPersons(personIds);
            var personById = persons.ToDictionary(x => x.Id, x => x);

            // 5. consolidate the data
            var contacts = GetContactByUprnForEachAsset(assets, tenuresByTenureId, personById, contactDetailsGroupedByTargetId);

            return contacts;
        }
    }
}
