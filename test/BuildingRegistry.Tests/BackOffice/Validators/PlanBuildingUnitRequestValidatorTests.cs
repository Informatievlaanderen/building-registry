namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentValidation.TestHelper;
    using Xunit;

    public class PlanBuildingUnitRequestValidatorTests
    {
        private readonly PlanBuildingUnitRequestValidator _validator;

        public PlanBuildingUnitRequestValidatorTests()
        {
            _validator = new PlanBuildingUnitRequestValidator();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void GivenInvalidGeometry_ThenReturnsExpectedFailure(string buildingId)
        {
            var result = _validator.TestValidate(new PlanBuildingUnitRequest
            {
                GebouwId = buildingId
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.GebouwId))
                .WithErrorCode(ValidationErrorCodes.BuildingUnit.BuildingNotFound)
                .WithErrorMessage(ValidationErrorMessages.BuildingUnit.BuildingInvalid(buildingId));
        }
    }
}
