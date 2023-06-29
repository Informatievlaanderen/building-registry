namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenMergingBuilding
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
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenValidRequest : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenValidRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
                .Setup(x => x.Send(It.IsAny<MergeBuildingsSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<MergeBuildingRequest>();

            var result = (AcceptedResult)await _controller.Merge(
                MockValidRequestValidator<MergeBuildingRequest>(),
                new MergeBuildingsSqsRequestFactory(new Mock<IPersistentLocalIdGenerator>().Object),
                request);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<MergeBuildingsSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Insert
                    ),
                    CancellationToken.None));
        }
    }
}
