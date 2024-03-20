namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
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
        public async Task GivenInvalidGeometry_ThenReturnsExpectedFailure()
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingRequest
            {
                GeometriePolygoon = ""
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }

        [Fact]
        public void WithSelfTouchingRing_ThenReturnsExpectedFailure()
        {
            var result = _validator.TestValidate(new PlanBuildingRequest
            {
                GeometriePolygoon = GeometryHelper.selfTouchingGml
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingRequest.GeometriePolygoon))
                .WithErrorCode("GebouwPolygoonValidatie")
                .WithErrorMessage("Ongeldig formaat geometriePolygoon.");
        }
    }
}
