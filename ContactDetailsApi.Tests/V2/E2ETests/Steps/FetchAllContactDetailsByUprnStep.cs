using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V2.E2ETests.Steps
{
    public class FetchAllContactDetailsByUprnStep : BaseSteps
    {
        public FetchAllContactDetailsByUprnStep(HttpClient httpClient) : base(httpClient)
        { }

        private async Task<HttpResponseMessage> CallApi()
        {
            var route = "api/v2/servicesoft";
            var uri = new Uri(route, UriKind.Relative);
            return await _httpClient.GetAsync(uri).ConfigureAwait(false);
        }

        public async Task WhenAllContactDetailsAreRequested()
        {
            _lastResponse = await CallApi().ConfigureAwait(false);
        }

        private async Task<List<ContactByUprn>> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<List<ContactByUprn>>(responseContent, _jsonOptions);
            return apiResult;
        }

        public async Task ThenAllContactDetailsAreReturned()
        {
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);
            apiResult.Should().NotBeNullOrEmpty();
            apiResult.Should().BeOfType<List<ContactByUprn>>();
        }

    }
}
