using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class DeleteContactDetailsSteps : BaseSteps
    {
        public DeleteContactDetailsSteps(HttpClient httpClient) : base(httpClient)
        {
        }
        private async Task<HttpResponseMessage> CallApi(string targetId, string id)
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImV2YW5nZWxvcy5ha3RvdWRpYW5ha2lzQGhhY2tuZXkuZ292LnVrIiwiaXNzIjoiSGFja25leSIsIm5hbWUiOiJFdmFuZ2Vsb3MgQWt0b3VkaWFuYWtpcyIsImdyb3VwcyI6WyJzYW1sLWF3cy1jb25zb2xlLW10ZmgtZGV2ZWxvcGVyIl0sImlhdCI6MTYyMzA1ODIzMn0.Jnd2kQTMiAUeKMJCYQVEVXbFc9BbIH90OociR15gfpw";
            var route = $"api/v1/contactDetails?targetId={targetId}&id={id}";
            var uri = new Uri(route, UriKind.Relative);

            var message = new HttpRequestMessage(HttpMethod.Delete, uri);

            message.Method = HttpMethod.Delete;
            message.Headers.Add("Authorization", token);

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _httpClient.SendAsync(message).ConfigureAwait(false);
        }

        private async Task<ContactDetailsResponseObject> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<ContactDetailsResponseObject>(responseContent, _jsonOptions);
            return apiResult;
        }

        public async Task WhenTheDeleteContactDetailsApiIsCalled(string targetId, string id)
        {
            _lastResponse = await CallApi(targetId, id).ConfigureAwait(false);
        }

        public async Task ThenTheContactDetailsAreDeleted(ContactDetailsFixture contactDetailsFixture)
        {
            var result = await contactDetailsFixture._dbContext.LoadAsync<ContactDetailsEntity>(contactDetailsFixture.TargetId, contactDetailsFixture.Contacts.First().Id).ConfigureAwait(false);
            result.IsActive.Should().BeFalse();

        }

        public void ThenNotFoundReturned()
        {
            _lastResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        public void ThenBadRequestReturned()
        {
            _lastResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }
}
