using ContactDetailsApi.Tests.V2.E2ETests.Fixtures;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Boundary.Response;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ContactDetailsEntity = ContactDetailsApi.V2.Infrastructure.ContactDetailsEntity;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ContactDetailsApi.Tests.V2.E2ETests.Steps
{
    public class CreateContactSteps : V1.E2ETests.Steps.BaseSteps
    {
        private const string Jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";

        public CreateContactSteps(HttpClient httpClient) : base(httpClient)
        { }

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

        public async Task ThenTheContactDetailsCreatedEventIsRaised(ISnsFixture snsFixture)
        {
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            Action<ContactDetailsSns> verifyFunc = (actual) =>
            {
                actual.CorrelationId.Should().NotBeEmpty();
                actual.DateTime.Should().BeCloseTo(DateTime.UtcNow, 1000);
                actual.EntityId.Should().Be(apiResult.TargetId);

                actual.EventData.NewData.ContactType.Should().Be((int) apiResult.ContactInformation.ContactType);
                actual.EventData.NewData.Description.Should().Be(apiResult.ContactInformation.Description);
                actual.EventData.NewData.Id.Should().Be(apiResult.Id);
                actual.EventData.NewData.Value.Should().Be(apiResult.ContactInformation.Value);
                actual.EventData.OldData.Should().BeEquivalentTo(new DataItem());

                actual.EventType.Should().Be(EventConstants.CREATED);
                actual.Id.Should().NotBeEmpty();
                actual.SourceDomain.Should().Be(EventConstants.SOURCEDOMAIN);
                actual.SourceSystem.Should().Be(EventConstants.SOURCESYSTEM);
                actual.User.Email.Should().Be("e2e-testing@development.com");
                actual.User.Name.Should().Be("Tester");
                actual.Version.Should().Be(EventConstants.V1VERSION);
            };

            var snsVerifer = snsFixture.GetSnsEventVerifier<ContactDetailsSns>();
            var snsResult = await snsVerifer.VerifySnsEventRaised(verifyFunc);
            if (!snsResult && snsVerifer.LastException != null)
                throw snsVerifer.LastException;
        }

        public async Task WhenTheCreateContactEndpointIsCalled(ContactDetailsRequestObject requestObject)
        {
            var route = $"api/v2/contactDetails";

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

        private async Task<List<string>> GetResponseErrorProperties()
        {
            var responseContent = await _lastResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                JObject jo = JObject.Parse(responseContent);

                // throws error if there are no errors
                var errorProperties = jo["errors"].Children().Select(x => x.Path.Split('.').Last().Trim('\'', ']')).ToList();

                return errorProperties;

            }
            catch (Exception)
            {
                return new List<string>();
            }

        }

        public async Task ThenThereIsAValidationErrorForField(string fieldName)
        {
            var errorProperties = await GetResponseErrorProperties().ConfigureAwait(false);

            errorProperties.Should().Contain(fieldName);
        }

        public async Task ThenThereIsNoValidationErrorForField(string fieldName)
        {
            var errorProperties = await GetResponseErrorProperties().ConfigureAwait(false);

            errorProperties.Should().NotContain(fieldName);
        }

        public async Task ThenTheContactDetailsAreSavedAndReturned(ContactDetailsFixture fixture)
        {
            var expected = fixture.ContactRequestObject;
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            var resultAsDb = ResponseToDatabase(apiResult);
            fixture.Contacts.Add(resultAsDb);
            expected.Should().BeEquivalentTo(apiResult, config =>
            {
                return config.Excluding(x => x.Id)
                    .Excluding(y => y.CreatedBy)
                    .Excluding(y => y.IsActive)
                    .Excluding(y => y.ContactInformation);
            });

            apiResult.Id.Should().NotBeEmpty();
            apiResult.IsActive.Should().BeTrue();
            apiResult.CreatedBy.Should().BeEquivalentTo(GetToken(Jwt).ToCreatedBy(), config => config.Excluding(x => x.CreatedAt));
            apiResult.CreatedBy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1000);

            var dbEntity = await fixture._dbContext.LoadAsync<ContactDetailsEntity>(apiResult.TargetId, apiResult.Id).ConfigureAwait(false);



            dbEntity.Should().BeEquivalentTo(resultAsDb, config => config.Excluding(x => x.LastModified));
            dbEntity.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);
        }

        public async Task TheMultilineAddressIsSavedInTheValueField(ContactDetailsFixture fixture)
        {
            var expected = fixture.ContactRequestObject;
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            var dbEntity = await fixture._dbContext.LoadAsync<ContactDetailsEntity>(apiResult.TargetId, apiResult.Id).ConfigureAwait(false);

            dbEntity.ContactInformation.ContactType.Should().Be(ContactType.address);

            // assert multiline address saved in value field
            dbEntity.ContactInformation.Value.Should().Contain(expected.ContactInformation.AddressExtended.AddressLine1);
            dbEntity.ContactInformation.Value.Should().Contain(expected.ContactInformation.AddressExtended.AddressLine2);
            dbEntity.ContactInformation.Value.Should().Contain(expected.ContactInformation.AddressExtended.AddressLine3);
            dbEntity.ContactInformation.Value.Should().Contain(expected.ContactInformation.AddressExtended.AddressLine4);
            dbEntity.ContactInformation.Value.Should().Contain(expected.ContactInformation.AddressExtended.PostCode);
        }

        public async Task TheMultilineAddressIsNotSavedInTheValueField(ContactDetailsFixture fixture)
        {
            var expected = fixture.ContactRequestObject;
            var apiResult = await ExtractResultFromHttpResponse(_lastResponse).ConfigureAwait(false);

            var dbEntity = await fixture._dbContext.LoadAsync<ContactDetailsEntity>(apiResult.TargetId, apiResult.Id).ConfigureAwait(false);

            dbEntity.ContactInformation.ContactType.Should().NotBe(ContactType.address);

            dbEntity.ContactInformation.Value.Should().Be(expected.ContactInformation.Value);

            // assert multiline address saved in value field
            dbEntity.ContactInformation.Value.Should().NotContain(expected.ContactInformation.AddressExtended.AddressLine1);
            dbEntity.ContactInformation.Value.Should().NotContain(expected.ContactInformation.AddressExtended.AddressLine2);
            dbEntity.ContactInformation.Value.Should().NotContain(expected.ContactInformation.AddressExtended.AddressLine3);
            dbEntity.ContactInformation.Value.Should().NotContain(expected.ContactInformation.AddressExtended.AddressLine4);
            dbEntity.ContactInformation.Value.Should().NotContain(expected.ContactInformation.AddressExtended.PostCode);
        }

        public async Task ThenTheResponseIncludesValidationErrors()
        {
            var errorProperties = await GetResponseErrorProperties().ConfigureAwait(false);

            errorProperties.Should().Contain("TargetId");
            errorProperties.Should().Contain("ContactInformation");
            errorProperties.Should().Contain("SourceServiceArea");
        }

        public async Task ThenTheResponseIncludesValidationErrorsForTooManyContacts()
        {
            var errorProperties = await GetResponseErrorProperties().ConfigureAwait(false);
            errorProperties.Should().Contain("ExistingContacts");
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
