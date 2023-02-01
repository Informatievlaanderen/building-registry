namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.Validation;
    using FluentValidation.TestHelper;
    using Moq;
    using SqlStreamStore;
    using Xunit;

    public class PlanBuildingUnitRequestValidatorTests
    {
        private readonly PlanBuildingUnitRequestValidator _validator;

        public PlanBuildingUnitRequestValidatorTests()
        {
            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamNotFound();
            _validator = new PlanBuildingUnitRequestValidator(new BuildingExistsValidator(streamStoreMock.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("http://bla/a")]
        [InlineData("http://bla/1")]
        public void GivenInvalidBuildingId_ThenReturnsExpectedFailure(string buildingId)
        {
            var result = _validator.TestValidate(new PlanBuildingUnitRequest
            {
                GebouwId = buildingId
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.GebouwId))
                .WithErrorCode(ValidationErrors.Common.BuildingNotFound.Code)
                .WithErrorMessage(ValidationErrors.Common.BuildingNotFound.InvalidGebouwId(buildingId));
        }
    }
}
