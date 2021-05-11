using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IGetContactDetailsByTargetIdUseCase
    {
        Task<List<ContactDetailsResponseObject>> Execute(ContactQueryParameter query);
    }
}
