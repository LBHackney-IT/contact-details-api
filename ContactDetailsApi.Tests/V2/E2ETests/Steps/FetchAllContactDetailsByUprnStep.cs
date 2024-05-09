using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.Shared.E2E;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Response;
using ContactDetailsApi.V2.Domain;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Shared.Tenure.Factories;
using Hackney.Shared.Tenure.Infrastructure;
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
            var route = "api/v2/servicesoft/contactDetails";
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

        private async Task<ContactsByUprnList> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<ContactsByUprnList>(responseContent, _jsonOptions);
            return apiResult;
        }

        public async Task ThenAllContactDetailsAreReturned(IDynamoDbFixture dbFixture)
        {
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);
            var results = apiResult.Results;
            results.Should().NotBeNullOrEmpty();
            results.Should().BeOfType<List<ContactByUprn>>();

            results.Should().SatisfyRespectively(x => x.Contacts.Should().NotBeNullOrEmpty());
            results.Should().SatisfyRespectively(x => x.Contacts.TrueForAll(x => x.IsResponsible));

            results.Should().OnlyContain(x => x.TenureId != null);
            foreach (var contactByUprn in results)
            {
                var tenure = await dbFixture.DynamoDbContext.LoadAsync<TenureInformationDb>(contactByUprn.TenureId.Value);
                tenure.ToDomain().IsActive.Should().BeTrue();
                contactByUprn.Address.Should().Be(tenure.TenuredAsset.FullAddress);
                contactByUprn.Uprn.Should().Be(tenure.TenuredAsset.Uprn);
            }
        }

        public void ThenAnUnauthorisedResponseIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
