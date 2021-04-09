using Amazon.XRay.Recorder.Core.Internal.Entities;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using System;
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

        public async Task<List<ContactDetailsResponseObject>> Execute(ContactQueryParameter cqp)
        {
            var contact = await _gateway.GetContactByTargetId(cqp.TargetId).ConfigureAwait(false);
            return contact.ToResponse();
        }
    }
}
