namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitPlanned : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitPlanned(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var buildings = new Mock<IBuildings>();
            var backOfficeContext = new Mock<BackOfficeContext>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(buildings.Object, backOfficeContext.Object);
        }

        [Fact]
        public async Task ThenShouldSucceed()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new PlanBuildingUnitResponse(buildingPersistentLocalId, buildingUnitPersistentLocalId, string.Empty));

            var request = new PlanBuildingUnitRequest()
            {
                GebouwId = $"/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.Plan(
                ResponseOptions,
                new PlanBuildingUnitRequestValidator(),
                request,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<PlanBuildingUnitRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }
    }
}
