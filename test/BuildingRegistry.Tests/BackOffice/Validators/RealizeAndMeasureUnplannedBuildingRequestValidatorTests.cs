namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class RealizeAndMeasureUnplannedBuildingRequestValidatorTests
    {
        private readonly RealizeAndMeasureUnplannedBuildingRequestValidator _validator;

        public RealizeAndMeasureUnplannedBuildingRequestValidatorTests()
        {
            _validator = new RealizeAndMeasureUnplannedBuildingRequestValidator();
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsNoValidationError()
        {
            var result = _validator.TestValidate(new RealizeAndMeasureUnplannedBuildingRequest
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
