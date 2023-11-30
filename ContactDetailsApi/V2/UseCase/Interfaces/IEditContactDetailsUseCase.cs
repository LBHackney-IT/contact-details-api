using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using Hackney.Core.JWT;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IEditContactDetailsUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(EditContactDetailsQuery query, EditContactDetailsRequest requestObject, string requestBody, Token token, int? ifMatch);
    }
}
