namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentValidation.TestHelper;
    using Moq;
    using SqlStreamStore;
    using Xunit;

    public class PlanBuildingUnitRequestValidatorTests
    {
        private readonly PlanBuildingUnitRequestValidator _validator;

        public PlanBuildingUnitRequestValidatorTests()
        {
            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamNotFound();
            _validator = new PlanBuildingUnitRequestValidator(new BuildingExistsValidator(streamStoreMock.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("http://bla/a")]
        [InlineData("http://bla/1")]
        public async Task GivenInvalidBuildingId_ThenReturnsExpectedFailure(string buildingId)
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingUnitRequest
            {
                GebouwId = buildingId
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.GebouwId))
                .WithErrorCode("GebouweenheidGebouwIdNietGekendValidatie")
                .WithErrorMessage($"De gebouwId '{buildingId}' is niet gekend in het gebouwenregister.");
        }

        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometry)]
        [InlineData(GeometryHelper.GmlPointGeometryLambert2008)]
        [InlineData(GeometryHelper.NormalizedGmlPointGeometry)]
        [InlineData(GeometryHelper.NormalizedGmlPointGeometryLambert2008)]
        public async Task GivenSupportedPositionReferenceSystem_ThenNoValidationErrorForPosition(string position)
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingUnitRequest
            {
                GebouwId = "https://data.vlaanderen.be/id/gebouw/1",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            });

            result.ShouldNotHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.Positie));
        }

        [Theory]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>4.35 50.85</gml:pos></gml:Point>")]
        [InlineData("<gml:Point srsName=\"https://INVALIDURL\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>")]
        [InlineData("<gml:Point missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>")]
        public async Task GivenUnsupportedPositionReferenceSystem_ThenReturnsExpectedFailure(string position)
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingUnitRequest
            {
                GebouwId = "https://data.vlaanderen.be/id/gebouw/1",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.Positie))
                .WithErrorCode("GebouweenheidPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }
    }
}
