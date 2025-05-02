namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenRemovingMeasuredBuilding
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
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingNotFound : BackOfficeApiTest
    {
        private readonly BuildingController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenBuildingNotFound(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                .Setup(x => x.Send(It.IsAny<RemoveMeasuredBuildingSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var request = Fixture.Create<RemoveMeasuredBuildingRequest>();
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult)await _controller.RemoveMeasuredBuilding(
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<RemoveMeasuredBuildingSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData!.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Delete
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            _streamStore.SetStreamFound();

            //Act
            var result = await _controller.RemoveMeasuredBuilding(
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(false),
                Fixture.Create<RemoveMeasuredBuildingRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public async Task WithNonExistingBuildingPersistentLocalId_ThenValidationErrorIsThrown()
        {
            //Arrange
            _streamStore.SetStreamNotFound();

            //Act
            var act = async () => await _controller.RemoveMeasuredBuilding(
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                Fixture.Create<RemoveMeasuredBuildingRequest>(),
                null,
                CancellationToken.None);

            //Assert
            await act
                .Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaand gebouw.");
        }
    }
}
