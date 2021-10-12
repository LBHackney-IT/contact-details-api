using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using Hackney.Core.JWT;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface ICreateContactUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject contactRequest, Token token);
    }
}
