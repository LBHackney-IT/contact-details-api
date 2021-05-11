using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using FluentAssertions;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void CanMapADatabaseEntityToADomainObject()
        {
            var domain = _fixture.Create<ContactDetails>();
            var response = domain.ToResponse();

            domain.Id.Should().Be(response.Id);
            domain.TargetId.Should().Be(response.TargetId);
            domain.TargetType.Should().Be(response.TargetType);
            domain.SourceServiceArea.Should().BeEquivalentTo(response.SourceServiceArea);
            domain.ContactInformation.Should().BeEquivalentTo(response.ContactInformation);
            domain.CreatedBy.Should().BeEquivalentTo(response.CreatedBy);
            domain.IsActive.Should().Be(response.IsActive);
            domain.RecordValidUntil.Should().Be(response.RecordValidUntil);
        }
    }
}
