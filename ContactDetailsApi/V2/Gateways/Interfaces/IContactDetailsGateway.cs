using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase;
using Hackney.Shared.Asset.Infrastructure;
using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface IContactDetailsGateway
    {
        Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query);
        Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails);
        Task<UpdateEntityResult<ContactDetailsEntity>> EditContactDetails(EditContactDetailsQuery query, EditContactDetailsRequest request, string requestBody);
        Task<List<ContactByUprn>> FetchAllContactDetailsByUprnUseCase(FetchAllContactDetailsQuery query);
        List<ContactByUprn> GetContactByUprnForEachAsset(List<ContactByUprn> assets,
                                                                 Dictionary<Guid, TenureInformationDb> tenuresByTenureId,
                                                                 Dictionary<Guid, PersonDbEntity> personById,
                                                                 Dictionary<Guid, List<ContactDetailsEntity>> contactDetailsGroupedByTargetId);

        Task<List<ContactByUprn>> FetchAllAssets(FetchAllContactDetailsQuery query);
        Task<List<PersonDbEntity>> FetchPersons(List<Guid> personIds);
        Task<List<TenureInformationDb>> FetchTenures(List<Guid?> tenureIds);
        Task<List<ContactDetailsEntity>> FetchAllContactDetails(FetchAllContactDetailsQuery query);


    }
}
