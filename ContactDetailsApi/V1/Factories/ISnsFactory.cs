using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Domain.Sns;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.Factories
{
    public interface ISnsFactory
    {
        ContactDetailsSns Create(ContactDetails contactDetails, Token token, string eventType);
    }
}
