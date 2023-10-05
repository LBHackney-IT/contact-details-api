using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Domain;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V2.Factories.Interfaces
{
    public interface ISnsFactory
    {
        ContactDetailsSns CreateEvent(ContactDetails newData, Token token);
        ContactDetailsSns EditEvent(ContactDetails newData, ContactDetails oldData, Token token);
    }
}
