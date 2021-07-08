using System.Threading.Tasks;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Factories;

namespace ContactDetailsApi.V1.Gateways
{
    public interface ISnsGateway
    {
        Task Publish(ContactDetailsSns contactDetailsSns);
    }
}
