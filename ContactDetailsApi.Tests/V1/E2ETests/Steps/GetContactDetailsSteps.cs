using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.Shared.E2E;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class GetContactDetailsSteps : BaseSteps
    {
        public GetContactDetailsSteps(HttpClient httpClient) : base(httpClient)
        { }

        private async Task<HttpResponseMessage> CallApi(string id, bool? includeHistoric = null)
        {
            var route = $"api/v1/contactDetails?targetId={id}";
            if (includeHistoric.HasValue)
                route += $"&includeHistoric={includeHistoric.Value}";
            var uri = new Uri(route, UriKind.Relative);
            return await _httpClient.GetAsync(uri).ConfigureAwait(false);
        }

        private async Task<GetContactDetailsResponse> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<GetContactDetailsResponse>(responseContent, _jsonOptions);
            return apiResult;
        }

        public async Task WhenTheContactDetailsAreRequested(string targetId, bool includeHistoric)
        {
            _lastResponse = await CallApi(targetId, includeHistoric).ConfigureAwait(false);
        }

        public async Task ThenTheContactDetailsAreReturned(List<ContactDetailsEntity> expectedContacts, bool includeHistoric)
        {
            var expected = expectedContacts;
            if (!includeHistoric)
                expected = expectedContacts.Where(x => x.IsActive).ToList();

            if (expected.Any())
            {
                var expectedAsResponses = expected.Select(x => x.ToDomain()).ToResponse();
                var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);
                apiResult.Results.Should().BeEquivalentTo(expectedAsResponses);
                apiResult.Results.Should().BeInAscendingOrder(x => x.CreatedBy.CreatedAt);
            }
            else
            {
                _lastResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }
    }
}
