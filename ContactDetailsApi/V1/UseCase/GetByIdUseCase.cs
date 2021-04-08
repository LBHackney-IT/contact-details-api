using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Gateways;
using ContactDetailsApi.V1.UseCase.Interfaces;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.UseCase
{
    public class GetByIdUseCase : IGetByIdUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        public GetByIdUseCase(IContactDetailsGateway gateway)
        {
            _gateway = gateway;
        }

        public async Task<ContactDetailsResponseObject> Execute(ContactQueryParameter cqr)
        {

            var contactResponse = await _gateway.GetEntityById(cqr.TargetId).ConfigureAwait(false);
            return contactResponse.ToResponse();
        }
    }
}
