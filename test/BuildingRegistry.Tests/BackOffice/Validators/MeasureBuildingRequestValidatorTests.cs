namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class MeasureBuildingRequestValidatorTests
    {
        private readonly MeasureBuildingRequestValidator _validator;

        public MeasureBuildingRequestValidatorTests()
        {
            _validator = new MeasureBuildingRequestValidator();
        }

        [Theory]
        [InlineData(GeometryHelper.GmlPolygonGeometry)]
        [InlineData(GeometryHelper.GmlPolygonGeometryLambert2008)]
        [InlineData(GeometryHelper.NormalizedGmlPolygonGeometry)]
        [InlineData(GeometryHelper.NormalizedGmlPolygonGeometryLambert2008)]
        public void GivenSupportedReferenceSystem_ThenReturnsNoValidationError(string polygon)
        {
            var result = _validator.TestValidate(new MeasureBuildingRequest
            {
                GrbData = new GrbData { GeometriePolygoon = polygon }
            });

            result.ShouldNotHaveValidationErrorFor($"{nameof(GrbData)}.{nameof(GrbData.GeometriePolygoon)}");
        }

        [Theory]
        [InlineData("<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>4.35 50.85 4.36 50.85 4.36 50.86 4.35 50.85</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        [InlineData("<gml:Polygon missingSrSNameAttribute=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15 186724.74 140291.06 186726.38 140288.22 186738.25 140284.15 186724.74</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        public void GivenUnsupportedReferenceSystem_ThenReturnsExpectedFailure(string polygon)
        {
            var result = _validator.TestValidate(new MeasureBuildingRequest
            {
                GrbData = new GrbData { GeometriePolygoon = polygon }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(GrbData)}.{nameof(GrbData.GeometriePolygoon)}")
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsNoValidationError()
        {
            var result = _validator.TestValidate(new MeasureBuildingRequest
            {
                GrbData = new GrbData()
                {
                    GeometriePolygoon = GeometryHelper.selfTouchingGml
                }
            });

            result.ShouldNotHaveValidationErrorFor($"{nameof(GrbData)}.{nameof(GrbData.GeometriePolygoon)}");
        }
    }
}
