namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitCannotBePlannedException : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitCannotBePlannedException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var buildings = new Mock<IBuildings>();
            var backOfficeContext = new Mock<BackOfficeContext>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(buildings.Object, backOfficeContext.Object);
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<PlanBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitCannotBePlannedException());

            var request = new PlanBuildingUnitRequest()
            {
                GebouwId = $"/{new BuildingPersistentLocalId(123)}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(
                ResponseOptions,
                new PlanBuildingUnitRequestValidator(),
                request,
                CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouwStatusOngeldig"
                    && e.ErrorMessage == "Een gebouweenheid kan enkel toegevoegd worden aan een gebouw in status: gepland, in aanbouw of gerealiseerd."));
        }
    }
}
