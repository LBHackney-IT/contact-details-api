using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Gateways
{
    public interface IContactDetailsGateway
    {
        Task<ContactDetails> CreateContact(ContactDetailsEntity contactDetails);
    }
}
