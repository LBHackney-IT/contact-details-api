using ContactDetailsApi.Tests.V1.E2ETests.Fixtures;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V1.E2ETests.Steps
{
    public class DeleteContactDetailsSteps : BaseSteps
    {
        public DeleteContactDetailsSteps(HttpClient httpClient) : base(httpClient)
        { }

        private async Task<HttpResponseMessage> CallApi(string targetId, string id)
        {
            var token =
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMTUwMTgxMTYwOTIwOTg2NzYxMTMiLCJlbWFpbCI6ImUyZS10ZXN0aW5nQGRldmVsb3BtZW50LmNvbSIsImlzcyI6IkhhY2tuZXkiLCJuYW1lIjoiVGVzdGVyIiwiZ3JvdXBzIjpbImUyZS10ZXN0aW5nIl0sImlhdCI6MTYyMzA1ODIzMn0.SooWAr-NUZLwW8brgiGpi2jZdWjyZBwp4GJikn0PvEw";
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

        public async Task WhenTheDeleteContactDetailsApiIsCalled(string targetId, string id)
        {
            _lastResponse = await CallApi(targetId, id).ConfigureAwait(false);
        }

        public async Task ThenTheContactDetailsAreDeleted(ContactDetailsFixture contactDetailsFixture)
        {
            var result = await contactDetailsFixture._dbContext.LoadAsync<ContactDetailsEntity>(contactDetailsFixture.TargetId,
                                                                                                contactDetailsFixture.Contacts.First().Id)
                                                    .ConfigureAwait(false);
            result.IsActive.Should().BeFalse();
            result.LastModified.Should().BeCloseTo(DateTime.UtcNow, 500);
        }

        public void ThenNotFoundReturned()
        {
            _lastResponse.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        public void ThenBadRequestReturned()
        {
            _lastResponse.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        public async Task ThenTheContactDetailsDeletedEventIsRaised(ContactDetailsFixture contactDetailsFixture,
                                                              ISnsFixture snsFixture)
        {
            var deletedRecord = contactDetailsFixture.Contacts.First();

            Action<ContactDetailsSns> verifyFunc = (actual) =>
            {
                actual.CorrelationId.Should().NotBeEmpty();
                actual.DateTime.Should().BeCloseTo(DateTime.UtcNow, 1000);
                actual.EntityId.Should().Be(deletedRecord.TargetId);

                actual.EventData.OldData.ContactType.Should().Be((int) deletedRecord.ContactInformation.ContactType);
                actual.EventData.OldData.Description.Should().Be(deletedRecord.ContactInformation.Description);
                actual.EventData.OldData.Id.Should().Be(deletedRecord.Id);
                actual.EventData.OldData.Value.Should().Be(deletedRecord.ContactInformation.Value);
                actual.EventData.NewData.Should().BeEquivalentTo(new DataItem());

                actual.EventType.Should().Be(EventConstants.DELETED);
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
    }
}
