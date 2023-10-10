using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Bogus;
using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V2.E2ETests.Steps
{
    public class PatchContactSteps : V1.E2ETests.Steps.BaseSteps
    {
        private const string Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";

        public PatchContactSteps(HttpClient httpClient) : base(httpClient)
        { }

        public async Task WhenThePatchContactEndpointIsCalled(EditContactDetailsRequest request, EditContactDetailsQuery query)
        {
            var route = $"api/v2/contactDetails/{query.PersonId}/update/{query.ContactDetailId}";

            var uri = new Uri(route, UriKind.Relative);

            var message = new HttpRequestMessage(HttpMethod.Post, uri);

            var jsonSettings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new[] { new StringEnumConverter() }
            };
            var requestJson = JsonConvert.SerializeObject(request, jsonSettings);


            message.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            message.Method = HttpMethod.Patch;
            message.Headers.Add("Authorization", Jwt);

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _lastResponse = await _httpClient.SendAsync(message).ConfigureAwait(false);
        }

        public void ThenA404NotFoundResponseIsReturned()
        {
            ThenAResponseIsReturned(StatusCodes.Status404NotFound);
        }

        public void ThenA204NoContentResponseIsReturned()
        {
            ThenAResponseIsReturned(StatusCodes.Status204NoContent);
        }

        private void ThenAResponseIsReturned(int statusCode)
        {
            _lastResponse.StatusCode.Should().Be(statusCode);
        }

        public async Task ThenTheContactDetailsAreUpdated(ContactDetailsFixture fixture)
        {
            var expected = fixture.ContactRequestObject;

            var dbEntity = await fixture._dbContext.LoadAsync<ContactDetailsEntity>(fixture.PatchContactDetailsQuery.PersonId, fixture.PatchContactDetailsQuery.ContactDetailId).ConfigureAwait(false);



            dbEntity.Should().BeEquivalentTo(
                fixture.PatchContactRequestObject.ToDatabase(),
                config => config.Excluding(x => x.LastModified));

            dbEntity.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);
        }
    }
}
