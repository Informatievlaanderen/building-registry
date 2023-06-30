namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Moq;
    using SqlStreamStore;
    using Xunit;

    public class MergeBuildingRequestValidatorTests
    {
        private readonly MergeBuildingRequestValidator _validator;
        private readonly Mock<IStreamStore> _streamStore;

        private const string ValidGml =
            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>";

        public MergeBuildingRequestValidatorTests()
        {
            _streamStore = new Mock<IStreamStore>();
            _validator = new MergeBuildingRequestValidator(new BuildingExistsValidator(_streamStore.Object));
        }

        [Fact]
        public void GivenValidRequest()
        {
            _streamStore.SetStreamFound();

            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = ValidGml,
                SamenvoegenGebouwen = new List<string>
                {
                    "http://validpuriformat/102",
                    "http://validpuriformat/103"
                }
            });

            result.ShouldNotHaveAnyValidationErrors();
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
                GeometriePolygoon = ValidGml,
                SamenvoegenGebouwen = new List<string>() { "1" }
            });

            result.ShouldHaveValidationErrorFor(nameof(MergeBuildingRequest.SamenvoegenGebouwen))
                .WithErrorCode("TooFewBuildings")
                .WithErrorMessage("TooFewBuildings");
        }

        [Fact]
        public void GivenInvalidBuildingPuri_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = ValidGml,
                SamenvoegenGebouwen = new List<string>
                {
                    "http://invalidp",
                    "http://invalidpuriformat./102"
                }
            });

            result.ShouldHaveValidationErrorFor(x => x.SamenvoegenGebouwen)
                .WithErrorCode("GebouwIdOngeldig")
                .WithErrorMessage("Ongeldig gebouwId.");
        }

        [Fact]
        public void GivenNonExistingBuilding_ThenReturnsExpectedFailure()
        {
            _streamStore.SetStreamNotFound();

            var puri = "http://validpuriformat/102";
            var result = _validator.TestValidate(new MergeBuildingRequest
            {
                GeometriePolygoon = ValidGml,
                SamenvoegenGebouwen = new List<string>
                {
                    puri
                }
            });

            result.ShouldHaveValidationErrorFor(x => x.SamenvoegenGebouwen)
                .WithErrorCode("GebouwIdNietGekendValidatie")
                .WithErrorMessage($"Het gebouwId '{puri}' is niet gekend in het gebouwenregister.");
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
                GeometriePolygoon = ValidGml,
                SamenvoegenGebouwen = samenvoegenGebouwen
            });

            result.ShouldHaveValidationErrorFor(nameof(MergeBuildingRequest.SamenvoegenGebouwen))
                .WithErrorCode("TooManyBuildings")
                .WithErrorMessage("TooManyBuildings");
        }
    }
}
