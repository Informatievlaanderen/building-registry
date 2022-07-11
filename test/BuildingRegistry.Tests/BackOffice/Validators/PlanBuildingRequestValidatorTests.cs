namespace BuildingRegistry.Tests.BackOffice.Validators
{
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
        public void GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = ""
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode(ValidationErrorCodes.Building.InvalidPolygonGeometry)
                .WithErrorMessage(ValidationErrorMessages.Building.InvalidPolygonGeometry);
        }
    }
}
