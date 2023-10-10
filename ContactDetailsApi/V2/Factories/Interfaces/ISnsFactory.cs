using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Infrastructure;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V2.Factories.Interfaces
{
    public interface ISnsFactory
    {
        ContactDetailsSns Create(ContactDetails contactDetails, Token token, string eventType);
    }
}
