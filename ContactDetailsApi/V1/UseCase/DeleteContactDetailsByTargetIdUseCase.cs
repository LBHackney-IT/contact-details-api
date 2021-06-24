using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using Hackney.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase
{
    public class DeleteContactDetailsByTargetIdUseCase : IDeleteContactDetailsByTargetIdUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        public DeleteContactDetailsByTargetIdUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task<ContactDetailsResponseObject> Execute(DeleteContactQueryParameter query)
        {
            var contact = await _gateway.DeleteContactDetailsById(query).ConfigureAwait(false);
            return contact.ToResponse();
        }
    }
}
