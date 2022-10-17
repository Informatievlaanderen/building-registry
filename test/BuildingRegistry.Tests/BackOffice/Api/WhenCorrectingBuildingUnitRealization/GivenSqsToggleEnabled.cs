namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitRealization
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.Exceptions;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(useSqs: true);
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRealizationSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.CorrectRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitRealizationRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.CorrectRealization(
                ResponseOptions,
                MockIfMatchValidator(false),
                new CorrectBuildingUnitRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitRealizationRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRealizationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitRealizationRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectRealization(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    new CorrectBuildingUnitRealizationRequestValidator(),
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
