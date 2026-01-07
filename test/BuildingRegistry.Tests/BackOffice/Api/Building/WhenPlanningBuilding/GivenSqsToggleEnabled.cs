namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenPlanningBuilding
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task WithPolygonDoublePoints_ThenTicketLocationIsReturnedWithCleanedUpPolygon()
        {
            var polygonWithDoublePoints =
                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>" +
                "232276.31667640081 185653.07371366365 " +
                "232278.52847587419 185660.05582659596 " +
                "232278.52847587419 185660.05582659596 " +
                "232289.95809909908 185656.54085878414 " +
                "232289.95809909908 185656.54085878414 " +
                "232287.80607798981 185649.53483450611 " +
                "232287.80607798981 185649.53483450611 " +
                "232276.31667640081 185653.07371366365 " +
                "232276.31667640081 185653.07371366365</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<PlanBuildingRequest>();
            request.GeometriePolygoon = polygonWithDoublePoints;

            var result = (AcceptedResult)await _controller.Plan(
                MockValidRequestValidator<PlanBuildingRequest>(),
                new PlanBuildingSqsRequestFactory(new Mock<IPersistentLocalIdGenerator>().Object),
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<PlanBuildingSqsRequest>(sqsRequest =>
                        sqsRequest.Request.GeometriePolygoon == "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>" +
                        "232276.31667640081 185653.07371366365 " +
                        "232278.52847587419 185660.05582659596 " +
                        "232289.95809909908 185656.54085878414 " +
                        "232287.80607798981 185649.53483450611 " +
                        "232276.31667640081 185653.07371366365</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<PlanBuildingRequest>();
            request.GeometriePolygoon = GeometryHelper.ValidPolygon.ToGmlJsonPolygon().Gml;

            var result = (AcceptedResult)await _controller.Plan(
                MockValidRequestValidator<PlanBuildingRequest>(),
                new PlanBuildingSqsRequestFactory(new Mock<IPersistentLocalIdGenerator>().Object),
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<PlanBuildingSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
        }
    }
}
