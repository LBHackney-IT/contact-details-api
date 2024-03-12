using ContactDetailsApi.Tests.V1.E2ETests.Steps;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Core.Middleware;
using Hackney.Core.Testing.Shared.E2E;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V2.E2ETests.Steps
{
    public class FetchAllContactDetailsByUprnStep : BaseSteps
    {
        public FetchAllContactDetailsByUprnStep(HttpClient httpClient) : base(httpClient)
        { }

        private async Task<HttpResponseMessage> CallApi()
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";
            var route = "api/v2/servicesoft";
            //if (!string.IsNullOrEmpty(paginationToken))
            //    route += $"&paginationToken={paginationToken}";
            //if (pageSize.HasValue)
            //    route += $"&pageSize={pageSize.Value}";
            var uri = new Uri(route, UriKind.Relative);

            var message = new HttpRequestMessage(HttpMethod.Get, uri);
            message.Method = HttpMethod.Get;
            message.Headers.Add("Authorization", token);

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return await _httpClient.SendAsync(message).ConfigureAwait(false);
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

        public async Task ThenUnauthorizedIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

    }
}
