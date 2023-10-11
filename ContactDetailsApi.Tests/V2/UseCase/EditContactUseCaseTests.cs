using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Factories.Interfaces;
using ContactDetailsApi.V2.UseCase;
using Hackney.Core.Sns;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using ContactDetailsApi.V2.Boundary.Request;
using Hackney.Core.JWT;
using ContactDetailsApi.V2.Infrastructure;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using ContactDetailsApi.V2.Factories;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Domain;

namespace ContactDetailsApi.Tests.V2.UseCase
{
    public class EditContactUseCaseTests
    {
        private readonly EditContactDetailsUseCase _classUnderTest;
        private readonly Mock<IContactDetailsGateway> _contactDetailsGatewayMock;
        private readonly Mock<ISnsGateway> _snsGatewayMock;
        private readonly Mock<ISnsFactory> _snsFactoryMock;

        private readonly Fixture _fixture = new Fixture();

        public EditContactUseCaseTests()
        {
            _contactDetailsGatewayMock = new Mock<IContactDetailsGateway>();
            _snsGatewayMock = new Mock<ISnsGateway>();
            _snsFactoryMock = new Mock<ISnsFactory>();

            _classUnderTest = new EditContactDetailsUseCase(
                _contactDetailsGatewayMock.Object,
                _snsGatewayMock.Object,
                _snsFactoryMock.Object
            );
        }

        [Fact]
        public async Task WhenNoChanges_DoesntPublishSnsEvent()
        {
            // Arrange
            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            var request = new EditContactDetailsRequest();
            var requestBody = string.Empty;
            var token = new Token();

            var gatewayResponse = new UpdateEntityResult<ContactDetailsEntity>
            {
                NewValues = new Dictionary<string, object>(),
                UpdatedEntity = _fixture.Create<ContactDetailsEntity>()
            };

            _contactDetailsGatewayMock
                .Setup(x => x.EditContactDetails(query, request, requestBody, null))
                .ReturnsAsync(gatewayResponse);

            // Act
            var response = await _classUnderTest.ExecuteAsync(query, request, requestBody, token, It.IsAny<int?>());

            // Assert
            response.Should().BeEquivalentTo(gatewayResponse.UpdatedEntity.ToDomain().ToResponse());

            _snsGatewayMock
                .Verify(x => x.Publish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task WhenChanges_PublishesSnsEvent()
        {
            // Arrange
            var query = new EditContactDetailsQuery
            {
                PersonId = Guid.NewGuid(),
                ContactDetailId = Guid.NewGuid()
            };

            var request = new EditContactDetailsRequest();
            var requestBody = string.Empty;
            var token = new Token();

            var newId = Guid.NewGuid();

            var gatewayResponse = new UpdateEntityResult<ContactDetailsEntity>
            {
                NewValues = new Dictionary<string, object>
                {
                    {"id", newId},
                },
                UpdatedEntity = _fixture.Create<ContactDetailsEntity>()
            };

            var snsFactoryResponse = _fixture.Create<ContactDetailsSns>();


            _contactDetailsGatewayMock
                .Setup(x => x.EditContactDetails(query, request, requestBody, null))
                .ReturnsAsync(gatewayResponse);

            _snsFactoryMock
                .Setup(x => x.Create(It.IsAny<ContactDetails>(), token, It.IsAny<string>()))
                .Returns(snsFactoryResponse);

            // Act
            var response = await _classUnderTest.ExecuteAsync(query, request, requestBody, token, It.IsAny<int?>());

            // Assert
            response.Should().BeEquivalentTo(gatewayResponse.UpdatedEntity.ToDomain().ToResponse());

            _snsGatewayMock
                .Verify(x => x.Publish(snsFactoryResponse, null, It.IsAny<string>()), Times.Once);
        }
    }
}
