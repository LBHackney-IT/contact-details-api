using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IDeleteContactDetailsByTargetIdUseCase
    {
        Task<ContactDetailsResponseObject> Execute(DeleteContactQueryParameter query);
    }
}
