namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using SqlStreamStore;
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
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var correctBuildingUnitPositionRequest = Fixture.Create<CorrectBuildingUnitPositionRequest>();

            var result = (AcceptedResult)await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                0,
                correctBuildingUnitPositionRequest,
                null,
                CancellationToken.None);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void WithNonExistingBuildingPersistentLocalId_ThenValidationErrorIsThrown()
        {
            var correctBuildingUnitPositionRequest = Fixture.Create<CorrectBuildingUnitPositionRequest>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            _streamStore.SetStreamNotFound();

            var request = Fixture.Create<CorrectBuildingUnitPositionRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectPosition(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                    0,
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
    }
}
