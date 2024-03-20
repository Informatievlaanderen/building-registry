namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
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
        public async Task GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = await _validator.TestValidateAsync(new ChangeBuildingOutlineRequest
            {
                GeometriePolygoon = ""
            });

            result.ShouldHaveValidationErrorFor(nameof(ChangeBuildingOutlineRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }
    }
}
