namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class CorrectBuildingMeasurementRequestValidatorTests
    {
        private readonly CorrectBuildingMeasurementRequestValidator _validator;

        public CorrectBuildingMeasurementRequestValidatorTests()
        {
            _validator = new CorrectBuildingMeasurementRequestValidator();
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsNoValidationError()
        {
            var result = _validator.TestValidate(new CorrectBuildingMeasurementRequest
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
