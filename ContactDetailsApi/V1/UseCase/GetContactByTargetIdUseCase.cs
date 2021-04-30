using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.Logging;
using ContactDetailsApi.V1.UseCase.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase
{
    public class GetContactByTargetIdUseCase : IGetContactByTargetIdUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        public GetContactByTargetIdUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }
        [LogCall]
        public async Task<List<ContactDetailsResponseObject>> Execute(ContactQueryParameter queryParam)
        {
            var contact = await _gateway.GetContactByTargetId(queryParam.TargetId).ConfigureAwait(false);
            return contact.ToResponse();
        }
    }
}
