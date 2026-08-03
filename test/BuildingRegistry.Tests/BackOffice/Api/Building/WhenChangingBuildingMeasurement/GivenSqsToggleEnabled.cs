namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenChangingBuildingMeasurement
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
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

        private ChangeBuildingMeasurementRequest CreateRequest(string polygon = GeometryHelper.GmlPolygonGeometry)
        {
            var request = Fixture.Create<ChangeBuildingMeasurementRequest>();
            request.GrbData.GeometriePolygoon = polygon;
            return request;
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingMeasurementSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var request = CreateRequest();

            var result = (AcceptedResult) await _controller.ChangeMeasurement(
                MockValidRequestValidator<ChangeBuildingMeasurementRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                Normalizer(),
                Fixture.Create<BuildingPersistentLocalId>(),
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ChangeBuildingMeasurementSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.Grb
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                    ),
                    CancellationToken.None));
        }

        [Theory]
        [InlineData(false, GeometryHelper.GmlPolygonGeometry, GeometryHelper.GmlPolygonGeometry)]
        [InlineData(false, GeometryHelper.GmlPolygonGeometryLambert2008, GeometryHelper.NormalizedGmlPolygonGeometry)]
        [InlineData(true, GeometryHelper.GmlPolygonGeometry, GeometryHelper.NormalizedGmlPolygonGeometryLambert2008)]
        [InlineData(true, GeometryHelper.GmlPolygonGeometryLambert2008, GeometryHelper.GmlPolygonGeometryLambert2008)]
        public async Task ThenPolygonIsSentInTheEventStoreReferenceSystem(
            bool useLambert2008EventStore,
            string requestedPolygon,
            string expectedPolygon)
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingMeasurementSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new LocationResult(CreateTicketUri(Fixture.Create<Guid>()))));

            _streamStore.SetStreamFound();

            await _controller.ChangeMeasurement(
                MockValidRequestValidator<ChangeBuildingMeasurementRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                Normalizer(useLambert2008EventStore),
                Fixture.Create<BuildingPersistentLocalId>(),
                CreateRequest(requestedPolygon));

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ChangeBuildingMeasurementSqsRequest>(sqsRequest =>
                        GmlAssertions.IsEquivalentGml(sqsRequest.Request.GrbData.GeometriePolygoon, expectedPolygon)),
                    CancellationToken.None));
        }

        [Fact]
        public void WithNonExistingBuildingPersistentLocalId_ThenThrowsApiException()
        {
            //Arrange
            _streamStore.SetStreamNotFound();

            //Act
            var act = async () => await _controller.ChangeMeasurement(
                MockValidRequestValidator<ChangeBuildingMeasurementRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                Normalizer(),
                Fixture.Create<BuildingPersistentLocalId>(),
                CreateRequest(),
                CancellationToken.None);

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
