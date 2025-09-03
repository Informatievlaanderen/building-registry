namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class WhenRepairingBuilding : BackOfficeLambdaTest
    {

        public WhenRepairingBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var commandHandlerResolver = new Mock<IIdempotentCommandHandler>();

            var handler = new RepairBuildingLambdaHandler(
                new FakeRetryPolicy(),
                ticketing.Object,
                commandHandlerResolver.Object);

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new RepairBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    new RepairBuildingSqsRequest
                    {
                        TicketId = ticketId,
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        ProvenanceData = Fixture.Create<ProvenanceData>()
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult("done"),
                    CancellationToken.None));

            commandHandlerResolver.Verify(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string,object>>(), It.IsAny<CancellationToken>()));
        }
    }
}
