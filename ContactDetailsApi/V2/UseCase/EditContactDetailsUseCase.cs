using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Factories.Interfaces;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
{
    public class EditContactDetailsUseCase : IEditContactDetailsUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;

        public EditContactDetailsUseCase(IContactDetailsGateway gateway, ISnsGateway snsGateway, ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        public async Task<ContactDetailsResponseObject> ExecuteAsync(
            EditContactDetailsQuery query,
            EditContactDetailsRequest request,
            string requestBody,
            Token token)
        {
            var result = await _gateway.EditContactDetails(query, request, requestBody).ConfigureAwait(false);
            if (result == null) return null;

            if (result.NewValues.Any() == true)
            {
                var assetSnsMessage = _snsFactory.Create(result.UpdatedEntity.ToDomain(), token, EventConstants.EDITED);
                var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");
                await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);
            }

            return result.UpdatedEntity.ToDomain().ToResponse();
        }
    }
}
