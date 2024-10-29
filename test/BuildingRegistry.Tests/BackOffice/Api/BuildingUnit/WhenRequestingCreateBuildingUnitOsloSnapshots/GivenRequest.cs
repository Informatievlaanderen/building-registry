namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenRequestingCreateOsloSnapshots
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Tests.BackOffice.Api;
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenRequest : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var buildingUnitPersistentLocalIds = Fixture.CreateMany<BuildingUnitPersistentLocalId>();

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(
                    It.IsAny<CreateBuildingUnitOsloSnapshotsSqsRequest>(),
                    CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = new CreateBuildingUnitOsloSnapshotsRequest
            {
                BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds.Select(x => (int)x).ToList(),
                Reden = "UnitTest"
            };

            var result = (AcceptedResult)await _controller.CreateOsloSnapshots(request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<CreateBuildingUnitOsloSnapshotsSqsRequest>(sqsRequest =>
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
