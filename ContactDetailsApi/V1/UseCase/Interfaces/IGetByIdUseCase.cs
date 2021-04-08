using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        Task<ContactDetailsResponseObject> Execute(ContactQueryParameter cqr);
    }
}
