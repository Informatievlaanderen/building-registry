namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class MergeBuildingRequestValidatorTests
    {
        private readonly MergeBuildingRequestValidator _validator;

        public MergeBuildingRequestValidatorTests()
        {
            _validator = new MergeBuildingRequestValidator();
        }

        [Fact]
        public void GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = "",
                SamenvoegenGebouwen = new List<string>()
            });

            result.ShouldHaveValidationErrorFor(nameof(MergeBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void GivenTooFewBuildings_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                SamenvoegenGebouwen = new List<string>(){"1"}
            });

            result.ShouldHaveValidationErrorFor(nameof(MergeBuildingRequest.SamenvoegenGebouwen))
                .WithErrorCode("TooFewBuildings")
                .WithErrorMessage("TooFewBuildings");
        }

        [Fact]
        public void GivenTooManyBuildings_ThenReturnsExpectedFailure()
        {
            var samenvoegenGebouwen = new List<string>();

            for (var i = 0; i < 21; i++)
            {
                samenvoegenGebouwen.Add(i.ToString());
            }

            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                SamenvoegenGebouwen = samenvoegenGebouwen
            });

            result.ShouldHaveValidationErrorFor(nameof(MergeBuildingRequest.SamenvoegenGebouwen))
                .WithErrorCode("TooManyBuildings")
                .WithErrorMessage("TooManyBuildings");
        }
    }
}