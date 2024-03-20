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
