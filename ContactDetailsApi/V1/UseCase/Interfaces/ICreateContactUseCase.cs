using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using Hackney.Core.JWT;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface ICreateContactUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token);
    }
}
