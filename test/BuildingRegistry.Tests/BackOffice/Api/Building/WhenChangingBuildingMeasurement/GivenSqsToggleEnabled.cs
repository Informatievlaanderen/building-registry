namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenChangingBuildingMeasurement
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Tests.Fixtures;
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
                .Setup(x => x.Send(It.IsAny<ChangeBuildingMeasurementSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var request = Fixture.Create<ChangeBuildingMeasurementRequest>();
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult) await _controller.ChangeMeasurement(
                MockValidRequestValidator<ChangeBuildingMeasurementRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                Fixture.Create<BuildingPersistentLocalId>(),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<ChangeBuildingMeasurementSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.Grb
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            _streamStore.SetStreamFound();

            //Act
            var result = await _controller.ChangeMeasurement(
                MockValidRequestValidator<ChangeBuildingMeasurementRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(false),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ChangeBuildingMeasurementRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
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
                MockIfMatchValidator(true),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ChangeBuildingMeasurementRequest>(),
                null,
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
