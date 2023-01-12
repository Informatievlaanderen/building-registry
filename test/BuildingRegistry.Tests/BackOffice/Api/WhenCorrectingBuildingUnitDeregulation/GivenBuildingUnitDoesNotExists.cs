namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitDeregulation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingDoesNotExist : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(useSqs: true);
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitDeregulationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitDeregulationRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectDeregulation(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    request,
                    string.Empty,
                    CancellationToken.None);
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
        public void WithBuildingUnitNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitDeregulationSqsRequest>(), CancellationToken.None))
                .Throws(new BuildingUnitIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitDeregulationRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectDeregulation(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    request,
                    string.Empty,
                    CancellationToken.None);
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
