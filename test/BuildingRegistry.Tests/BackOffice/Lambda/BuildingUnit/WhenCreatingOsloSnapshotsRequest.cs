namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.AllStream;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Tests.BackOffice;
    using BuildingRegistry.Tests.BackOffice.Lambda;
    using FluentAssertions;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenCreatingOsloSnapshotsRequest : BackOfficeLambdaTest
    {
        private readonly IdempotencyContext _idempotencyContext;

        public WhenCreatingOsloSnapshotsRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext([]);
        }

        [Fact]
        public async Task ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new CreateOsloSnapshotsLambdaHandler(
                new FakeRetryPolicy(),
                ticketing.Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext));

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new CreateOsloSnapshotsLambdaRequest(
                    AllStreamId.Instance,
                    new CreateOsloSnapshotsSqsRequest
                    {
                        TicketId = ticketId,
                        Request = new CreateOsloSnapshotsRequest
                        {
                            BuildingUnitPersistentLocalIds = [1]
                        },
                        ProvenanceData = Fixture.Create<ProvenanceData>()
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult("done"),
                    CancellationToken.None));

            //Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(AllStreamId.Instance), 0, 1);
            var message = stream.Messages.First();
            message.JsonMetadata.Should().Contain(Provenance.ProvenanceMetadataKey.ToLower());
        }

        [Fact]
        public async Task WhenIdempotencyException_ThenTicketingCompleteIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new CreateOsloSnapshotsLambdaHandler(
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler(() => new IdempotencyException(string.Empty)).Object);

            // Act
            var ticketId = Guid.NewGuid();
            await handler.Handle(
                new CreateOsloSnapshotsLambdaRequest(
                    AllStreamId.Instance,
                    new CreateOsloSnapshotsSqsRequest
                    {
                        TicketId = ticketId,
                        Request = new CreateOsloSnapshotsRequest
                        {
                            BuildingUnitPersistentLocalIds = [1]
                        },
                        ProvenanceData = Fixture.Create<ProvenanceData>()
                    }),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Complete(
                    ticketId,
                    new TicketResult("done"),
                    CancellationToken.None));
        }
    }
}
