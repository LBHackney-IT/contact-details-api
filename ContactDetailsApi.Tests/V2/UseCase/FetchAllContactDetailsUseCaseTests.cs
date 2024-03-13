using AutoFixture;
using ContactDetailsApi.V1.Boundary.Request;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.UseCase;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Linq;
using ContactDetailsApi.V2.Domain;
using Xunit;
using ContactDetailsApi.V2.Infrastructure;
using FluentAssertions;
using Hackney.Shared.Tenure.Domain;
using Person = Hackney.Shared.Person.Person;

namespace ContactDetailsApi.Tests.V2.UseCase
{
    [Collection("LogCall collection")]
    public class FetchAllContactDetailsUseCaseTests
    {
        private readonly FetchAllContactDetailsByUprnUseCase _classUnderTest;
        private readonly Mock<ITenureDbGateway> _mockTenureGateway;
        private readonly Mock<IPersonDbGateway> _mockPersonGateway;
        private readonly Mock<IContactDetailsGateway> _mockContactDetailsGateway;

        private readonly Fixture _fixture = new Fixture();

        public FetchAllContactDetailsUseCaseTests()
        {
            _mockTenureGateway = new Mock<ITenureDbGateway>();
            _mockPersonGateway = new Mock<IPersonDbGateway>();
            _mockContactDetailsGateway = new Mock<IContactDetailsGateway>();

            _classUnderTest = new FetchAllContactDetailsByUprnUseCase(_mockTenureGateway.Object,
                _mockPersonGateway.Object, _mockContactDetailsGateway.Object);

        }

        [Fact]
        public void ConsolidateDataReturnsListOfContacts()
        {
            // Arrange
            var person = _fixture.Create<Person>();
            var contactDetails = _fixture.Build<ContactDetails>().With(x => x.TargetId, person.Id).CreateMany().ToList();
            var householdMembers = new List<HouseholdMembers>
            {
                _fixture.Build<HouseholdMembers>().With(x => x.Id, person.Id).Create()
            };
            var tenure = _fixture.Build<TenureInformation>().With(x => x.HouseholdMembers, householdMembers).Create();

            var tenures = new List<TenureInformation> { tenure };
            var persons = new Dictionary<Guid, Person> { { person.Id, person } };
            var contactDetailsDict = new Dictionary<Guid, IEnumerable<ContactDetails>> { { person.Id, contactDetails } };

            // Act
            var result = _classUnderTest.ConsolidateData(tenures, persons, contactDetailsDict);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<ContactByUprn>>();
            result.Should().HaveCount(1);

            var resultItem = result.First();
            resultItem.Uprn.Should().Be(tenure.TenuredAsset.Uprn);
            resultItem.TenureId.Should().Be(tenure.Id);
            resultItem.Contacts.Should().NotBeNullOrEmpty();
            resultItem.Contacts.Should().HaveCount(1);

            var contact = resultItem.Contacts.First();
            contact.PersonTenureType.Should().Be(householdMembers.First().PersonTenureType);
            contact.IsResponsible.Should().Be(householdMembers.First().IsResponsible);
            contact.FirstName.Should().Be(person.FirstName);
            contact.LastName.Should().Be(person.Surname);
            contact.Title.Should().Be(person.Title);

            var personContactDetails = contact.PersonContactDetails;
            personContactDetails.Should().NotBeNullOrEmpty();
            personContactDetails.Should().HaveCount(contactDetails.Count);
        }

        [Fact]
        public void ConsolidateDataReturnsListOfContactsWhenNoContactDetails()
        {
            // Arrange
            var person = _fixture.Create<Person>();
            var householdMembers = new List<HouseholdMembers>
            {
                _fixture.Build<HouseholdMembers>().With(x => x.Id, person.Id).Create()
            };
            var tenure = _fixture.Build<TenureInformation>().With(x => x.HouseholdMembers, householdMembers).Create();

            var tenures = new List<TenureInformation> { tenure };
            var persons = new Dictionary<Guid, Person> { { person.Id, person } };
            var contactDetailsDict = new Dictionary<Guid, IEnumerable<ContactDetails>>();

            // Act
            var result = _classUnderTest.ConsolidateData(tenures, persons, contactDetailsDict);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<ContactByUprn>>();
            result.Should().HaveCount(1);

            var resultItem = result.First();
            resultItem.Uprn.Should().Be(tenure.TenuredAsset.Uprn);
            resultItem.TenureId.Should().Be(tenure.Id);
            resultItem.Contacts.Should().NotBeNullOrEmpty();
            resultItem.Contacts.Should().HaveCount(1);

            var contact = resultItem.Contacts.First();
            contact.PersonTenureType.Should().Be(householdMembers.First().PersonTenureType);
            contact.IsResponsible.Should().Be(householdMembers.First().IsResponsible);
            contact.FirstName.Should().Be(person.FirstName);
            contact.LastName.Should().Be(person.Surname);
            contact.Title.Should().Be(person.Title);
        }

        [Fact]
        public async Task ExecuteAsyncReturnsListOfContacts()
        {
            // Arrange
            var person = _fixture.Create<Person>();
            var contactDetails = _fixture.Build<ContactDetails>().With(x => x.TargetId, person.Id).CreateMany().ToList();
            var householdMembers = new List<HouseholdMembers>
            {
                _fixture.Build<HouseholdMembers>().With(x => x.Id, person.Id).Create()
            };
            var tenure = _fixture.Build<TenureInformation>().With(x => x.HouseholdMembers, householdMembers).Create();

            _mockTenureGateway.Setup(x => x.GetAllTenures()).ReturnsAsync(new List<TenureInformation> { tenure });
            _mockPersonGateway.Setup(x => x.GetPersons(new List<Guid> { person.Id })).ReturnsAsync(new List<Person> { person });
            _mockContactDetailsGateway.Setup(x => x.GetContactDetailsByTargetId(It.IsAny<ContactQueryParameter>())).ReturnsAsync(contactDetails);

            // Act
            var result = await _classUnderTest.ExecuteAsync().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<ContactByUprn>>();
            result.Should().HaveCount(1); // 1 tenure

            result.First().Contacts.Should().HaveCount(1); // 1 household member / person
            result.First().Contacts.First().PersonContactDetails.Should().HaveCount(contactDetails.Count); // count of contact details
        }
    }
}
