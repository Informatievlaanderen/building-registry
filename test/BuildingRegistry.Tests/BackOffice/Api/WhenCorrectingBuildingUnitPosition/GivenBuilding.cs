namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;

    public class GivenBuilding : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;
        private readonly Mock<IStreamStore> _streamStoreMock;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var buildings = new Mock<IBuildings>();
            var backOfficeContext = new Mock<BackOfficeContext>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
            _streamStoreMock = new Mock<IStreamStore>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithPositionGeometryMethodAppointedByAdministratorAndMissingPosition_ThenValidationExceptionIsThrown(string? position)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            var request = new CorrectBuildingUnitPositionRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            };

            _streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                0,
                request,
                null,
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

            var request = new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>"
            };

            _streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                0,
                request,
                null,
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
