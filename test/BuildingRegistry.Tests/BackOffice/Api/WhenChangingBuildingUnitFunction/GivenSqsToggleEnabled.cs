namespace BuildingRegistry.Tests.BackOffice.Api.WhenChangingBuildingUnitFunction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
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
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<ChangeBuildingUnitFunctionRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(false),
                new ChangeBuildingUnitFunctionRequestValidator(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<ChangeBuildingUnitFunctionRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            Func<Task> act = async () =>
            {
                await _controller.ChangeFunction(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    new ChangeBuildingUnitFunctionRequestValidator(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    Fixture.Create<ChangeBuildingUnitFunctionRequest>(),
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
