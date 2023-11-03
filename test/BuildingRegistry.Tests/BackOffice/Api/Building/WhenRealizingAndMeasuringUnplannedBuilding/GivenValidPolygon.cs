namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenRealizingAndMeasuringUnplannedBuilding
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Fixtures;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenValidPolygon : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenValidPolygon(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeAndMeasureUnplannedBuildingSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<RealizeAndMeasureUnplannedBuildingRequest>();

            await _controller.RealizeAndMeasureUnplannedBuilding(
                MockValidRequestValidator<RealizeAndMeasureUnplannedBuildingRequest>(),
                new RealizeAndMeasureUnplannedBuildingSqsRequestFactory(new Mock<IPersistentLocalIdGenerator>().Object),
                request);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<RealizeAndMeasureUnplannedBuildingSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.Grb
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
        }
    }
}
