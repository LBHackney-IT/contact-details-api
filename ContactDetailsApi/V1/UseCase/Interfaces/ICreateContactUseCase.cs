using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface ICreateContactUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(ContactDetailsRequestObject query, Token token, string eventType);
    }
}
