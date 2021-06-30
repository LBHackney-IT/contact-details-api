using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface ICreateContactUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject query);
    }
}
