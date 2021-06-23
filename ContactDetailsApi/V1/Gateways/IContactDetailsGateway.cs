using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Gateways
{
    public interface IContactDetailsGateway
    {
        Task<List<ContactDetails>> GetContactDetailsByTargetId(ContactQueryParameter query);

        Task<ContactDetails> CreateContact(ContactDetailsRequestObject requestObject);
    }
}
