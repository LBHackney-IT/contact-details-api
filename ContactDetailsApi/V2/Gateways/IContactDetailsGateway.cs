using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{
    public interface IContactDetailsGateway
    {
        Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query);
        Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails);
    }
}
