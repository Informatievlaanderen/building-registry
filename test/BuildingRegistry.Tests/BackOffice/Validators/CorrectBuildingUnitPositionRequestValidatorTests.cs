namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class CorrectBuildingUnitPositionRequestValidatorTests
    {
        private readonly CorrectBuildingUnitPositionRequestValidator _validator;

        public CorrectBuildingUnitPositionRequestValidatorTests()
        {
            _validator = new CorrectBuildingUnitPositionRequestValidator();
        }

        [Theory]
        [InlineData(GeometryHelper.GmlPointGeometry)]
        [InlineData(GeometryHelper.GmlPointGeometryLambert2008)]
        [InlineData(GeometryHelper.NormalizedGmlPointGeometry)]
        [InlineData(GeometryHelper.NormalizedGmlPointGeometryLambert2008)]
        public void GivenSupportedPositionReferenceSystem_ThenNoValidationErrorForPosition(string position)
        {
            var result = _validator.TestValidate(new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            });

            result.ShouldNotHaveValidationErrorFor(nameof(CorrectBuildingUnitPositionRequest.Positie));
        }

        [Theory]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>4.35 50.85</gml:pos></gml:Point>")]
        [InlineData("<gml:Point srsName=\"https://INVALIDURL\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>")]
        [InlineData("<gml:Point missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>141299.00 185188.00</gml:pos></gml:Point>")]
        public void GivenUnsupportedPositionReferenceSystem_ThenReturnsExpectedFailure(string position)
        {
            var result = _validator.TestValidate(new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectBuildingUnitPositionRequest.Positie))
                .WithErrorCode("GebouweenheidPositieformaatValidatie")
                .WithErrorMessage("De positie is geen geldige gml-puntgeometrie.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GivenNoPositionAndPositionGeometryMethodIsAppointedByAdministrator_ThenReturnsExpectedFailure(string? position)
        {
            var result = _validator.TestValidate(new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            });

            result.ShouldHaveValidationErrorFor(nameof(CorrectBuildingUnitPositionRequest.Positie))
                .WithErrorCode("GebouweendheidPositieValidatie")
                .WithErrorMessage("De verplichte parameter 'positie' ontbreekt.");
        }
    }
}
