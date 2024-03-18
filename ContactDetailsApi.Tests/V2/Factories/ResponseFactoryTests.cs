using AutoFixture;
using ContactDetailsApi.V2.Domain;
using ContactDetailsApi.V2.Factories;
using FluentAssertions;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Factories
{
    public class ResponseFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CanMapANullDomainContactDetailsToADomainObject()
        {
            // Arrange
            ContactDetails domain = null;

            // Act
            var response = domain.ToResponse();

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void CanMapADomainContactDetailsToAResponseObject()
        {
            // Arrange
            var domain = _fixture.Create<ContactDetails>();

            // Act
            var response = domain.ToResponse();

            // Assert
            domain.Id.Should().Be(response.Id);
            domain.TargetId.Should().Be(response.TargetId);
            domain.TargetType.Should().Be(response.TargetType);
            domain.SourceServiceArea.Should().BeEquivalentTo(response.SourceServiceArea);
            domain.ContactInformation.Should().BeEquivalentTo(response.ContactInformation);
            domain.CreatedBy.Should().BeEquivalentTo(response.CreatedBy);
            domain.IsActive.Should().Be(response.IsActive);
            domain.RecordValidUntil.Should().Be(response.RecordValidUntil);
        }

        [Fact]
        public void CanMapAListOfContactByUprnsToAResponseObject()
        {
            // Arrange
            var domainList = _fixture.CreateMany<ContactByUprn>();

            // Act
            var response = domainList.ToResponse();

            // Assert
            response.Results.Should().BeEquivalentTo(domainList);
        }
    }
}
