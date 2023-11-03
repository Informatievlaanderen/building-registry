namespace BuildingRegistry.Tests.BackOffice.Sqs.BuildingUnit
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using FluentAssertions;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit;
    using BuildingRegistry.Building;
    using Fixtures;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenBackOfficeAttachAddressToBuildingUnitRequest : BuildingRegistryTest
    {
        private readonly FakeBackOfficeContext _backOfficeContext;

        public GivenBackOfficeAttachAddressToBuildingUnitRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            await _backOfficeContext.AddBuildingUnitBuilding(Fixture.Create<BuildingPersistentLocalId>(), Fixture.Create<BuildingUnitPersistentLocalId>());

            var sut = new AttachAddressToBuildingUnitSqsHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl,
                _backOfficeContext);

            var sqsRequest = new AttachAddressToBuildingUnitSqsRequest()
            {
                BuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>(),
                Request = new AttachAddressToBuildingUnitRequest()
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == Fixture.Create<BuildingPersistentLocalId>().ToString()),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }


        [Fact]
        public void WithNoBuildingFoundByBuildingUnitPersistentLocalId_ThenThrowsAggregateIdNotFound()
        {
            // Arrange
            var sut = new AttachAddressToBuildingUnitSqsHandler(
                Mock.Of<ISqsQueue>(),
                Mock.Of<ITicketing>(),
                Mock.Of<ITicketingUrl>(),
                _backOfficeContext);

            // Act
            var act = async () => await sut.Handle(
                new AttachAddressToBuildingUnitSqsRequest()
                {
                    BuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>(),
                    Request = new AttachAddressToBuildingUnitRequest()
                }, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
