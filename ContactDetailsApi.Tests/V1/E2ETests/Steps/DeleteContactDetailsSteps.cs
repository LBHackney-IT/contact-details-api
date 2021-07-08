using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class DeleteContactDetailsSteps : BaseSteps
    {
        public DeleteContactDetailsSteps(HttpClient httpClient) : base(httpClient)
        {
        }
        private async Task<HttpResponseMessage> CallApi(string targetId, string id)
        {
            var route = $"api/v1/contactDetails?targetId={targetId}&id={id}";
            var uri = new Uri(route, UriKind.Relative);
            return await _httpClient.DeleteAsync(uri).ConfigureAwait(false);
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
