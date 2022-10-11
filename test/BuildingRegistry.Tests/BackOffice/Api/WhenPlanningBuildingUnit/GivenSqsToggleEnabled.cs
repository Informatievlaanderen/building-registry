namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _streamStore = new Mock<IStreamStore>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(useSqs: true);
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.Setup(x => x.ListStreams(It.IsAny<Pattern>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ListStreamsPage("1", new[] { "1" }, (_, _) => null));

            var planBuildingUnitRequest = Fixture.Create<PlanBuildingUnitRequest>();
            planBuildingUnitRequest.GebouwId = "https://bla/1";

            var result = (AcceptedResult)await _controller.Plan(
                ResponseOptions,
                MockValidRequestValidator<PlanBuildingUnitRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                planBuildingUnitRequest);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithNonExistingBuildingPersistentLocalId_ThenValidationErrorIsThrown()
        {
            var planBuildingUnitRequest = Fixture.Create<PlanBuildingUnitRequest>();
            planBuildingUnitRequest.GebouwId = "https://bla/1";

            //Act
            var act = async () => await _controller.Plan(
                ResponseOptions,
                MockValidRequestValidator<PlanBuildingUnitRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                planBuildingUnitRequest,
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
