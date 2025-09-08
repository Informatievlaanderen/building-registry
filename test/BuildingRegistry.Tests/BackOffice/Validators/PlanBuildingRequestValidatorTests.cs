namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class PlanBuildingRequestValidatorTests
    {
        private readonly PlanBuildingRequestValidator _validator;

        public PlanBuildingRequestValidatorTests()
        {
            _validator = new PlanBuildingRequestValidator();
        }

        [Fact]
        public async Task GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingRequest
            {
                GeometriePolygoon = ""
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = GeometryHelper.selfTouchingGml
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void WithSmallBuilding_ThenReturnsExpectedFailure()
        {
            var smallGmlPolygon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>0 0 0 0.5 0.5 0.5 0.5 0 0 0</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = smallGmlPolygon
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwTeKlein")
                .WithErrorMessage("De aangeleverde polygoon voor het gebouw heeft een oppervlakte van minder dan 1m².");
        }
    }
}
