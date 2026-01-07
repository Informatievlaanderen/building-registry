namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenChangingBuildingOutline
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _streamStore = new Mock<IStreamStore>();
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingOutlineSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var request = Fixture.Create<ChangeBuildingOutlineRequest>();
            request.GeometriePolygoon = GeometryHelper.ValidPolygon.ToGmlJsonPolygon().Gml;
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult) await _controller.ChangeOutline(
                MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                Fixture.Create<BuildingPersistentLocalId>(),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ChangeBuildingOutlineSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            _streamStore.SetStreamFound();

            var changeBuildingOutlineRequest = Fixture.Create<ChangeBuildingOutlineRequest>();
            changeBuildingOutlineRequest.GeometriePolygoon = GeometryHelper.ValidPolygon.ToGmlJsonPolygon().Gml;

            //Act
            var result = await _controller.ChangeOutline(
                MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(false),
                Fixture.Create<BuildingPersistentLocalId>(),
                changeBuildingOutlineRequest,
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
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
                .Setup(x => x.Send(It.IsAny<ChangeBuildingOutlineSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var expectedIfMatchHeader = Fixture.Create<string>();
            var request = Fixture.Create<ChangeBuildingOutlineRequest>();
            request.GeometriePolygoon = polygonWithDoublePoints;

            var result = (AcceptedResult)await _controller.ChangeOutline(
                MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                Fixture.Create<BuildingPersistentLocalId>(),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ChangeBuildingOutlineSqsRequest>(sqsRequest =>
                        sqsRequest.Request.GeometriePolygoon == "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>" +
                        "232276.31667640081 185653.07371366365 " +
                        "232278.52847587419 185660.05582659596 " +
                        "232289.95809909908 185656.54085878414 " +
                        "232287.80607798981 185649.53483450611 " +
                        "232276.31667640081 185653.07371366365</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public void WithNonExistingBuildingPersistentLocalId_ThenThrowsApiException()
        {
            //Arrange
            _streamStore.SetStreamNotFound();

            //Act
            var act = async () =>
            {
                var changeBuildingOutlineRequest = Fixture.Create<ChangeBuildingOutlineRequest>();
                changeBuildingOutlineRequest.GeometriePolygoon = GeometryHelper.ValidPolygon.ToGmlJsonPolygon().Gml;

                return await _controller.ChangeOutline(
                    MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                    new BuildingExistsValidator(_streamStore.Object),
                    MockIfMatchValidator(true),
                    Fixture.Create<BuildingPersistentLocalId>(),
                    changeBuildingOutlineRequest,
                    null,
                    CancellationToken.None);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaand gebouw.");
        }
    }
}
