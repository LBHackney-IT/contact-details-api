using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using System.Threading.Tasks;
using Hackney.Core.JWT;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IDeleteContactDetailsByTargetIdUseCase
    {
        Task<ContactDetailsResponseObject> Execute(DeleteContactQueryParameter query, Token token, string eventType);
    }
}
