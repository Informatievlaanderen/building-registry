namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenRemovingBuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<RemoveBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<RemoveBuildingUnitRequest>();
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult)await _controller.Remove(
                MockIfMatchValidator(true),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<RemoveBuildingUnitSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Delete
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.Remove(
                MockIfMatchValidator(false),
                Fixture.Create<RemoveBuildingUnitRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<RemoveBuildingUnitSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<RemoveBuildingUnitRequest>();
            Func<Task> act = async () =>
            {
                await _controller.Remove(
                    MockIfMatchValidator(true),
                    request,
                    string.Empty);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenAggregateNotFoundException_ThenThrowApiException()
        {
            var buildingPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            var request = new RemoveBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<RemoveBuildingUnitSqsRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException(buildingPersistentLocalId, typeof(Building)));

            //Act
            Func<Task> act = async () => await _controller.Remove(
                MockIfMatchValidator(true),
                request,
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }
    }
}
