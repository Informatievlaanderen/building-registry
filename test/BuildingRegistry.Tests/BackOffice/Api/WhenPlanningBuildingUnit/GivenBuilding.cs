namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var buildings = new Mock<IBuildings>();
            var backOfficeContext = new Mock<BackOfficeContext>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenAcceptedResponseIsExpected()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new PlanBuildingUnitResponse(buildingUnitPersistentLocalId, string.Empty));

            var request = new PlanBuildingUnitRequest()
            {
                GebouwId = $"https://data.vlaanderen.be/id/gebouw/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
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

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void
            WithPositionGeometryMethodAppointedByAdministratorAndMissingPosition_ThenValidationExceptionIsThrown(string? position)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position,
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(
                ResponseOptions,
                new PlanBuildingUnitRequestValidator(),
                request,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweendheidPositieValidatie"
                    && e.ErrorMessage == "De verplichte parameter 'positie' ontbreekt."));
        }

        [Fact]
        public void WithPositionHavingInvalidFormat_ThenValidationExceptionIsThrown()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new PlanBuildingUnitRequest()
            {
                GebouwId = $"/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(
                ResponseOptions,
                new PlanBuildingUnitRequestValidator(),
                request,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidPositieformaatValidatie"
                    && e.ErrorMessage == "De positie is geen geldige gml-puntgeometrie."));
        }
    }
}
