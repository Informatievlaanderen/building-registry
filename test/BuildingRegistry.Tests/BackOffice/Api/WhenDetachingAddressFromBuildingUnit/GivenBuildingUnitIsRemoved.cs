namespace BuildingRegistry.Tests.BackOffice.Api.WhenDetachingAddressFromBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using Building.Datastructures;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsRemoved : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void GivenBuildingUnitIsRemovedException_ThenThrowApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException());

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest(),
                    null,
                    CancellationToken.None);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Verwijderde gebouweenheid."
                    && x.StatusCode == StatusCodes.Status410Gone);
        }
    }
}
