using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using Hackney.Core.JWT;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IEditContactUseCase
    {
        Task<ContactDetailsResponseObject> ExecuteAsync(Guid contactDetailsId, EditContactDetailsRequest requestObject, string requestBody, Token token, int? ifMatch);
    }
}
