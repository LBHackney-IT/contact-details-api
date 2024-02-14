using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using Hackney.Core.Logging;
using Hackney.Shared.Asset.Domain;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{
    public class ContactDetailsDynamoDbGateway : IContactDetailsGateway
    {
        private readonly IDynamoDBContext _dynamoDbContext;
        private readonly ILogger<ContactDetailsDynamoDbGateway> _logger;
        private readonly IEntityUpdater _updater;

        public ContactDetailsDynamoDbGateway(IDynamoDBContext dynamoDbContext, ILogger<ContactDetailsDynamoDbGateway> logger, IEntityUpdater updater)
        {
            _dynamoDbContext = dynamoDbContext;
            _logger = logger;
            _updater = updater;
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


        //private async Task<List<AssetDb>> FetchAllAssets()
        //{
        //    var result = await _dynamoDbContext.LoadAsync<AssetDb>(new Guid("ec761910-399f-a6e6-6af8-af9ac8f535e4")).ConfigureAwait(false);


        //    var assets = new List<AssetDb>();

        //    var scanConditions = new List<ScanCondition>();
        //    var scan = _dynamoDbContext.ScanAsync<AssetDb>(scanConditions);

        //    do
        //    {
        //        try
        //        {
        //            var newResults = await scan.GetNextSetAsync().ConfigureAwait(false);
        //            assets.AddRange(newResults);
        //        }
        //        catch (Exception e)
        //        {
        //            var x = e;
        //            //throw;
        //        }
        //    }
        //    while (!scan.IsDone);


        //    return assets;
        //}

        private async Task<List<ContactDetailsEntity>> FetchAllContactDetails()
        {
            var scanConditions = new List<ScanCondition>();
            var results = new List<ContactDetailsEntity>();

            var scan = _dynamoDbContext.ScanAsync<ContactDetailsEntity>(scanConditions);

            //new Guid("986a2a9e-9eb4-0966-120a-238689e3e265")/
            var val = await _dynamoDbContext.LoadAsync<object>(new DynamoDBOperationConfig
            {
                IndexName = "Assets",

            });

            do
            {
                try
                {
                    var newResults = await scan.GetNextSetAsync().ConfigureAwait(false);
                    results.AddRange(newResults);
                }
                catch (Exception e)
                {
                    var x = e;
                    //throw;
                }
            }
            while (!scan.IsDone);

            return results;
        }

        private async Task<List<TenureInformationDb>> FetchTenures(List<Guid> tenureIds)
        {
            var tenureBatchRequest = _dynamoDbContext.CreateBatchGet<TenureInformationDb>();

            foreach (var id in tenureIds)
            {
                tenureBatchRequest.AddKey(id);
            }

            await tenureBatchRequest.ExecuteAsync().ConfigureAwait(false);

            return tenureBatchRequest.Results;
        }

        private async Task<List<PersonDbEntity>> FetchPersons(List<Guid> personIds)
        {
            var personBatchRequest = _dynamoDbContext.CreateBatchGet<PersonDbEntity>();

            foreach (var id in personIds)
            {
                personBatchRequest.AddKey(id);
            }

            await personBatchRequest.ExecuteAsync();

            return personBatchRequest.Results;
        }

        private async Task<List<Infrastructure.ContactByUprn>> FetchAllAssets()
        {
            var client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.EUWest2
            });

            var table = Table.LoadTable(client, "Assets");
            //var document = await table.GetItemAsync("986a2a9e-9eb4-0966-120a-238689e3e265");

            var search = table.Scan(new ScanOperationConfig
            {

            });

            var rawResults = new List<Document>();

            do
            {
                var nextResults = await search.GetNextSetAsync();

                rawResults.AddRange(nextResults);
            } while (!search.IsDone);



            var results = new List<ContactByUprn>();

            foreach (var result in rawResults)
            {
                try
                {
                    var assetAddress = result["assetAddress"].AsDocument();
                    var uprn = assetAddress.ContainsKey("uprn") ? assetAddress["uprn"] : null;

                    var tenure = result.ContainsKey("tenure") ? result["tenure"].AsDocument() : null;
                    var tenureId = tenure != null && tenure.ContainsKey("id") ? tenure["id"] : null;



                    var entity = new ContactByUprn
                    {
                        Uprn = uprn?.ToString() ?? "",
                        // TenureId = tenureId == null ? null : new Guid(tenureId)
                    };

                    results.Add(entity);
                }
                catch (Exception e)
                {
                    var x = e;
                    //throw;
                }
            }

            return results;
        }

        public async Task<List<ContactByUprn>> FetchAllContactDetailsByUprnUseCase()
        {
            // only select relevant fields
            var assets = await FetchAllAssets();

            // remove assets without a uprn
            var filteredAssets = assets
                .Where(x => !string.IsNullOrWhiteSpace(x.Uprn))
                .ToList();




            // 1. Scan all assets
            //var contactDetails = await FetchAllContactDetails();
            //var assets = await FetchAllAssets();

            // 2. Fetch all contact details

            //var contactDetailsGroupedByTargetId = contactDetails
            //    .GroupBy(deetz => deetz.TargetId)
            //    .ToDictionary(group => group.Key, group => group.ToList());

            //// 2. Fetch tenure records
            //var tenureIds = assets.Select(x => new Guid(x.Tenure.Id)).ToList();
            //var tenures = await FetchTenures(tenureIds);
            //var tenuresByTenureId = tenures.ToDictionary(x => x.Id, x => x);

            //// 3. For each household member, fetch contact details, and person recond
            //var personIds = tenures.SelectMany(tenure => tenure.HouseholdMembers.Select(x => x.Id)).ToList();
            //var persons = await FetchPersons(personIds);
            //var personById = persons.ToDictionary(x => x.Id, x => x);

            //// 5. consolidate the data
            //var contacts = new List<ContactByUprn>();

            //foreach (var asset in assets)
            //{
            //    var tenure = tenuresByTenureId[new Guid(asset.Tenure.Id)];

            //    var personContacts = new List<PersonContact>();

            //    foreach (var householdMember in tenure.HouseholdMembers)
            //    {
            //        var person = personById[householdMember.Id];

            //        var deets = contactDetailsGroupedByTargetId[person.Id]
            //            .Select(x => new PersonContactDeets
            //            {
            //                ContactType = x.ContactInformation.ContactType.ToString(),
            //                SubType = x.ContactInformation.SubType.ToString(),
            //                Value = x.ContactInformation.Value.ToString()
            //            })
            //            .ToList();


            //        var personContact = new PersonContact
            //        {
            //            PersonTenureType = householdMember.PersonTenureType.ToString(),
            //            IsResponsible = householdMember.IsResponsible,
            //            FirstName = person.FirstName,
            //            LastName = person.Surname,
            //            Title = person.Title.ToString(),
            //            Deets = deets
            //        };

            //        personContacts.Add(personContact);                    
            //    }

            //    var contactByUprn = new ContactByUprn
            //    {
            //        Uprn = asset.AssetAddress.Uprn,
            //        TenureId = tenure.Id,
            //        Contacts = personContacts
            //    };

            //    contacts.Add(contactByUprn);

            //}
            return assets;

            //return contacts;
        }
    }
}
