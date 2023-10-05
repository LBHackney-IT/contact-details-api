using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways.Interfaces
{
    public interface IContactDetailsGateway
    {
        Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query);
        Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails);
        Task<EditContactDetailsDomain> EditContactDetails(Guid assetId, EditContactDetailsRequest request, string requestBody, int? ifMatch);
    }
}
