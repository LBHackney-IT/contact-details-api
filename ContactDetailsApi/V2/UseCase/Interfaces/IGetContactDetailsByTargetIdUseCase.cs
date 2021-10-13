using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IGetContactDetailsByTargetIdUseCase
    {
        Task<List<ContactDetailsResponseObject>> Execute(ContactQueryParameter query);
    }
}
