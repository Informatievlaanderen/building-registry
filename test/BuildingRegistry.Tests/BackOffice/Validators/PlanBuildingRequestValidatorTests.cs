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

        [Theory]
        [InlineData(GeometryHelper.GmlPolygonGeometry)]
        [InlineData(GeometryHelper.GmlPolygonGeometryLambert2008)]
        [InlineData(GeometryHelper.NormalizedGmlPolygonGeometry)]
        [InlineData(GeometryHelper.NormalizedGmlPolygonGeometryLambert2008)]
        public void GivenSupportedReferenceSystem_ThenNoValidationErrorForGeometry(string polygon)
        {
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = polygon
            });

            result.ShouldNotHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon));
        }

        [Theory]
        [InlineData("<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>4.35 50.85 4.36 50.85 4.36 50.86 4.35 50.85</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        [InlineData("<gml:Polygon srsName=\"https://INVALIDURL\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15 186724.74 140291.06 186726.38 140288.22 186738.25 140284.15 186724.74</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        [InlineData("<gml:Polygon missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15 186724.74 140291.06 186726.38 140288.22 186738.25 140284.15 186724.74</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        public void GivenUnsupportedReferenceSystem_ThenReturnsExpectedFailure(string polygon)
        {
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = polygon
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
