using System;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using ContactDetailsApi.V1.Factories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ContactDetailsApi.V1.Gateways
{
    public class ContactDetailsSnsGateway : ISnsGateway
    {
        private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;
        private readonly IConfiguration _configuration;

        public ContactDetailsSnsGateway(IAmazonSimpleNotificationService amazonSimpleNotificationService, IConfiguration configuration)
        {
            _amazonSimpleNotificationService = amazonSimpleNotificationService;
            _configuration = configuration;
        }

        public async Task Publish(ContactDetailsSns contactDetailsSns)
        {
            string message = JsonConvert.SerializeObject(contactDetailsSns);
            var request = new PublishRequest
            {
                Message = message,
                TopicArn = Environment.GetEnvironmentVariable("CONTACT_DETAILS_SNS_ARN"),
                MessageGroupId = "SomeGroupId"
            };

            await _amazonSimpleNotificationService.PublishAsync(request).ConfigureAwait(false);
        }
    }
}
