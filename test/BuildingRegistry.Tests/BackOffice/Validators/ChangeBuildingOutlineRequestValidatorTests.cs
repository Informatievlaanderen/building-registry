namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.Validation;
    using FluentValidation.TestHelper;
    using Xunit;

    public class ChangeBuildingOutlineRequestValidatorTests
    {
        private readonly ChangeBuildingOutlineRequestValidator _validator;

        public ChangeBuildingOutlineRequestValidatorTests()
        {
            _validator = new ChangeBuildingOutlineRequestValidator();
        }

        [Fact]
        public void GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new ChangeBuildingOutlineRequest
            {
                GeometriePolygoon = ""
            });

            result.ShouldHaveValidationErrorFor(nameof(ChangeBuildingOutlineRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }
    }
}
