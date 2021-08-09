using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Hackney.Core.JWT;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class CreateContactSteps : BaseSteps
    {
        private const string Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImV2YW5nZWxvcy5ha3RvdWRpYW5ha2lzQGhhY2tuZXkuZ292LnVrIiwiaXNzIjoiSGFja25leSIsIm5hbWUiOiJFdmFuZ2Vsb3MgQWt0b3VkaWFuYWtpcyIsImdyb3VwcyI6WyJzYW1sLWF3cy1jb25zb2xlLW10ZmgtZGV2ZWxvcGVyIl0sImlhdCI6MTYyMzA1ODIzMn0.Jnd2kQTMiAUeKMJCYQVEVXbFc9BbIH90OociR15gfpw";

        public CreateContactSteps(HttpClient httpClient) : base(httpClient)
        { }

        private async Task<HttpResponseMessage> CallApi(string id, bool? includeHistoric = null)
        {
            var route = $"api/v1/contactDetails?targetId={id}";
            if (includeHistoric.HasValue)
                route += $"&includeHistoric={includeHistoric.Value}";
            var uri = new Uri(route, UriKind.Relative);
            return await _httpClient.GetAsync(uri).ConfigureAwait(false);
        }

        private async Task<GetContactDetailsResponse> ExtractGetResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<GetContactDetailsResponse>(responseContent, _jsonOptions);
            return apiResult;
        }

        private static ContactDetailsEntity ResponseToDatabase(ContactDetailsResponseObject response)
        {
            return new ContactDetailsEntity
            {
                Id = response.Id,
                TargetId = response.TargetId,
                TargetType = response.TargetType,
                ContactInformation = response.ContactInformation,
                SourceServiceArea = response.SourceServiceArea,
                CreatedBy = response.CreatedBy,
                IsActive = response.IsActive,
                RecordValidUntil = response.RecordValidUntil
            };
        }

        public async Task WhenTheCreateContactEndpointIsCalled(ContactDetailsRequestObject requestObject)
        {
            var route = $"api/v1/contactDetails";

            var uri = new Uri(route, UriKind.Relative);

            var message = new HttpRequestMessage(HttpMethod.Post, uri);

            message.Content = new StringContent(JsonConvert.SerializeObject(requestObject), Encoding.UTF8, "application/json");
            message.Method = HttpMethod.Post;
            message.Headers.Add("Authorization", Jwt);

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _lastResponse = await _httpClient.SendAsync(message).ConfigureAwait(false);
        }

        public async Task ThenTheContactDetailsAreSavedAndReturned(ContactDetailsFixture fixture)
        {
            var expected = fixture.Contact;
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            fixture.Contacts.Add(ResponseToDatabase(apiResult));
            expected.Should().BeEquivalentTo(apiResult, config => config.Excluding(x => x.Id)
                                                                        .Excluding(y => y.CreatedBy)
                                                                        .Excluding(y => y.IsActive));
            apiResult.Id.Should().NotBeEmpty();
            apiResult.IsActive.Should().BeTrue();
            apiResult.CreatedBy.Should().BeEquivalentTo(GetToken(Jwt).ToCreatedBy(), config => config.Excluding(x => x.CreatedAt));
            apiResult.CreatedBy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1000);

            var getResponse = await CallApi(apiResult.TargetId.ToString(), true).ConfigureAwait(false);
            var getResults = await ExtractGetResultFromHttpResponse(getResponse).ConfigureAwait(false);
            getResults.Results.Count.Should().Be(1);
            getResults.Results.First().Should().BeEquivalentTo(apiResult);
        }

        public async Task ThenTheResponseIncludesValidationErrors()
        {
            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            JObject jo = JObject.Parse(responseContent);
            var errorProperties = jo["errors"].Children().Select(x => x.Path.Split('.').Last().Trim('\'', ']')).ToList();

            errorProperties.Should().Contain("TargetId");
            errorProperties.Should().Contain("ContactInformation");
            errorProperties.Should().Contain("SourceServiceArea");
        }

        public void ThenBadRequestIsReturned()
        {
            _lastResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        public async Task ThenBadRequestValidationErrorResultIsReturned(string propertyName)
        {
            await ThenBadRequestValidationErrorResultIsReturned(propertyName, null, null).ConfigureAwait(false);
        }
        public async Task ThenBadRequestValidationErrorResultIsReturned(string propertyName, string errorCode)
        {
            await ThenBadRequestValidationErrorResultIsReturned(propertyName, errorCode, null).ConfigureAwait(false);
        }
        public async Task ThenBadRequestValidationErrorResultIsReturned(string propertyName, string errorCode, string errorMsg)
        {
            _lastResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var resultBody = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            resultBody.Should().Contain("One or more validation errors occurred");
            resultBody.Should().Contain(propertyName);
            if (null != errorCode)
                resultBody.Should().Contain(errorCode);
            if (null != errorMsg)
                resultBody.Should().Contain(errorMsg);
        }

        private async Task<ContactDetailsResponseObject> ExtractResultFromHttpResponse(HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var apiResult = JsonSerializer.Deserialize<ContactDetailsResponseObject>(responseContent, _jsonOptions);

            return apiResult;
        }

        private static Token GetToken(string jwt)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(jwt);
            var decodedPayload = Base64UrlEncoder.Decode(jwtToken.EncodedPayload);
            return JsonConvert.DeserializeObject<Token>(decodedPayload);
        }
    }
}
