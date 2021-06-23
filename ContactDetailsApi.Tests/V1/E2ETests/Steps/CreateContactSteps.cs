using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ContactDetailsApi.V1.Boundary.Request;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class CreateContactSteps : BaseSteps
    {
        public CreateContactSteps(HttpClient httpClient) : base(httpClient)
        { }

        public async Task WhenTheCreateContactEndpointIsCalled(ContactDetailsRequestObject requestObject)
        {
            var route = $"api/v1/contactDetails";

            var uri = new Uri(route, UriKind.Relative);

            var message = new HttpRequestMessage(HttpMethod.Post, uri);

            message.Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
            message.Method = HttpMethod.Post;

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _lastResponse = await _httpClient.SendAsync(message).ConfigureAwait(false);
        }

        public async Task ThenTheContactDetailsAreReturned(ContactDetailsRequestObject expectedContacts)
        {
            var expected = expectedContacts;

            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);
            apiResult.Should().BeEquivalentTo(expected);
        }

        private async Task<ContactDetailsResponseObject> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<ContactDetailsResponseObject>(responseContent, _jsonOptions);

            return apiResult;
        }
    }
}
