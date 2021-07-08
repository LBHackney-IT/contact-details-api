using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Domain.Sns;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.Factories
{
    public interface ISnsFactory
    {
        ContactDetailsSns Create(ContactDetailsRequestObject contactDetails, Token token);
    }
}
