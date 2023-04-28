namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenMeasuringBuilding
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
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BackOfficeApiTest
    {
        private readonly BuildingController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingControllerWithUser<BuildingController>();
            _streamStore = new Mock<IStreamStore>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<MeasureBuildingSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var request = Fixture.Create<MeasureBuildingRequest>();

            var result = (AcceptedResult)await _controller.Measure(
                MockValidRequestValidator<MeasureBuildingRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                buildingPersistentLocalId,
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<MeasureBuildingSqsRequest>(sqsRequest =>
                        sqsRequest.BuildingPersistentLocalId == buildingPersistentLocalId
                        && sqsRequest.Request == request
                        && sqsRequest.IfMatchHeaderValue == null
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.Grb
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public void WithInvalidPolygon_ThenThrowsValidationException()
        {
            var act = async () => await _controller.Measure(
                new MeasureBuildingRequestValidator(),
                new BuildingExistsValidator(_streamStore.Object),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<MeasureBuildingRequest>());

            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouwPolygoonValidatie"
                    && e.ErrorMessage == "Ongeldig formaat geometriePolygoon."));
        }
    }
}
