using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Factories.Interfaces;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using Hackney.Core.JWT;
using Hackney.Core.Sns;
using System;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase
{
    public class EditContactUseCase : IEditContactUseCase
    {
        private readonly IContactDetailsGateway _gateway;
        private readonly ISnsGateway _snsGateway;
        private readonly ISnsFactory _snsFactory;

        public EditContactUseCase(IContactDetailsGateway gateway, ISnsGateway snsGateway, ISnsFactory snsFactory)
        {
            _gateway = gateway;
            _snsGateway = snsGateway;
            _snsFactory = snsFactory;
        }

        public async Task<ContactDetailsResponseObject> ExecuteAsync(
            Guid contactDetailsId, EditContactDetailsRequest request, string requestBody, Token token, int? ifMatch)
        {
            var result = await _gateway.EditContactDetails(contactDetailsId, request, requestBody, ifMatch).ConfigureAwait(false);
            if (result == null) return null;

            //if (result.UpdateResult?.NewValues.Any())
            //{
            //    var assetSnsMessage = _snsFactory.EditEvent(result.UpdatedEntity, result.OldValues, token, EventConstants.EDITED);
            //    var assetTopicArn = Environment.GetEnvironmentVariable("ASSET_SNS_ARN");
            //    await _snsGateway.Publish(assetSnsMessage, assetTopicArn).ConfigureAwait(false);
            //}

            return result.UpdateResult.UpdatedEntity.ToDomain().ToResponse();
        }
    }
}