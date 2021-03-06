using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;
using FluentAssertions;
using Hackney.Core.JWT;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Factories
{
    public class ContactDetailsSnsFactoryTests
    {
        private readonly ContactDetailsSnsFactory _sut;

        public ContactDetailsSnsFactoryTests()
        {
            _sut = new ContactDetailsSnsFactory();
        }

        [Fact]
        public void GivenContactDetailsWhenUnsupportedEventShouldThrow()
        {
            // arrange
            var contactDetails = CreateContactDetails();
            var eventType = "Unsupported";

            // act
            Action act = () => _sut.Create(contactDetails, new Token(), eventType);

            // assert
            act.Should().Throw<NotImplementedException>().WithMessage($"Event {eventType} not recognized");
        }

        [Fact]
        public void GivenContactDetailsWhenCreatingEventShouldPopulateNewData()
        {
            // arrange
            var contactDetails = CreateContactDetails();
            var eventType = EventConstants.CREATED;

            // act
            var result = _sut.Create(contactDetails, new Token(), eventType);

            // assert
            result.EventData.OldData.Value.Should().BeNull();
            result.EventData.NewData.Id.Should().Be(contactDetails.Id);
            result.EventData.NewData.Value.Should().Be(contactDetails.ContactInformation.Value);
            result.EventData.NewData.ContactType.Should().Be((int) contactDetails.ContactInformation.ContactType);
            result.EventData.NewData.Description.Should().Be(contactDetails.ContactInformation.Description);
        }

        [Fact]
        public void GivenContactDetailsWhenDeletingEventShouldPopulateOldData()
        {
            // arrange
            var contactDetails = CreateContactDetails();
            var eventType = EventConstants.DELETED;

            // act
            var result = _sut.Create(contactDetails, new Token(), eventType);

            // assert
            result.EventData.NewData.Value.Should().BeNull();
            result.EventData.OldData.Id.Should().Be(contactDetails.Id);
            result.EventData.OldData.Value.Should().Be(contactDetails.ContactInformation.Value);
            result.EventData.OldData.ContactType.Should().Be((int) contactDetails.ContactInformation.ContactType);
            result.EventData.OldData.Description.Should().Be(contactDetails.ContactInformation.Description);
        }

        private static ContactDetails CreateContactDetails()
        {
            return new ContactDetails
            {
                ContactInformation = new ContactInformation
                {
                    Value = "SomeValue",
                    ContactType = ContactType.address,
                    Description = "SomeDescription"
                }
            };
        }
    }
}
