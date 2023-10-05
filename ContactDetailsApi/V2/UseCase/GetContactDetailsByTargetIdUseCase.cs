using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
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
