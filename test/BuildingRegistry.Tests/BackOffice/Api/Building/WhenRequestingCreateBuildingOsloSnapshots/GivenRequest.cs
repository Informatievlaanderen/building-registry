namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenRequestingCreateOsloSnapshots
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using Api;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var buildingPersistentLocalIds = Fixture.CreateMany<BuildingPersistentLocalId>();

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(
                    It.IsAny<CreateBuildingOsloSnapshotsSqsRequest>(),
                    CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = new CreateBuildingOsloSnapshotsRequest
            {
                BuildingPersistentLocalIds = buildingPersistentLocalIds.Select(x => (int)x).ToList(),
                Reden = "Test"
            };

            var result = (AcceptedResult)await _controller.CreateOsloSnapshots(request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CreateBuildingOsloSnapshotsSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Unknown
                        && sqsRequest.ProvenanceData.Reason == request.Reden
                    ),
                    CancellationToken.None));
        }
    }
}
