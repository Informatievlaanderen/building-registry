namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitRegularization
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using BuildingRegistry.Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRegularizationSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitRegularizationRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectRegularization(
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
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRegularizationSqsRequest>(), CancellationToken.None))
                .Throws(new BuildingUnitIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitRegularizationRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectRegularization(
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
