using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using FluentAssertions;
using Hackney.Core.JWT;
using Hackney.Shared.Sns;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Factories
{
    public class ContactDetailsSnsFactoryTests
    {
        private ContactDetailsSnsFactory _sut;

        public ContactDetailsSnsFactoryTests()
        {
            _sut = new ContactDetailsSnsFactory();
        }

        [Fact]
        public void GivenContactDetailsWhenCreatingEventShouldPopulateNewData()
        {
            // arrange
            var contactDetails = new ContactDetails
            {
                ContactInformation = new ContactInformation
                {
                    Value = "SomeValue"
                }
            };
            var eventType = ContactDetailsConstants.CREATED;

            // act
            var result = _sut.Create(contactDetails, new Token(), eventType);

            // assert
            result.EventData.OldData.Value.Should().BeNull();
            result.EventData.NewData.Value.Should().Be(contactDetails.ContactInformation.Value);
        }

        [Fact]
        public void GivenContactDetailsWhenDeletingEventShouldPopulateOldData()
        {
            // arrange
            var contactDetails = new ContactDetails
            {
                ContactInformation = new ContactInformation
                {
                    Value = "SomeValue"
                }
            };
            var eventType = ContactDetailsConstants.DELETED;

            // act
            var result = _sut.Create(contactDetails, new Token(), eventType);

            // assert
            result.EventData.NewData.Value.Should().BeNull();
            result.EventData.OldData.Value.Should().Be(contactDetails.ContactInformation.Value);
        }
    }
}
