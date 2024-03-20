namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class ChangeBuildingMeasurementRequestValidatorTests
    {
        private readonly ChangeBuildingMeasurementRequestValidator _validator;

        public ChangeBuildingMeasurementRequestValidatorTests()
        {
            _validator = new ChangeBuildingMeasurementRequestValidator();
        }

        [Fact]
        public void GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new ChangeBuildingMeasurementRequest
            {
                GrbData = new GrbData
                {
                    GeometriePolygoon = ""
                }
            });

            result.ShouldHaveValidationErrorFor($"{nameof(GrbData)}.{nameof(GrbData.GeometriePolygoon)}")
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new ChangeBuildingMeasurementRequest
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
