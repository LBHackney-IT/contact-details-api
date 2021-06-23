using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase
{
    public class GetContactDetailsByTargetIdUseCase : IGetContactDetailsByTargetIdUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        public GetContactDetailsByTargetIdUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task<List<ContactDetailsResponseObject>> Execute(ContactQueryParameter query)
        {
            var contact = await _gateway.GetContactDetailsByTargetId(query).ConfigureAwait(false);
            return contact.ToResponse();
        }
    }
}
