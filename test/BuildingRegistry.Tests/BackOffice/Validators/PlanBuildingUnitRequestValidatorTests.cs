namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
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
        public async Task GivenInvalidBuildingId_ThenReturnsExpectedFailure(string buildingId)
        {
            var result = await _validator.TestValidateAsync(new PlanBuildingUnitRequest
            {
                GebouwId = buildingId
            });

            result.ShouldHaveValidationErrorFor(nameof(PlanBuildingUnitRequest.GebouwId))
                .WithErrorCode("GebouweenheidGebouwIdNietGekendValidatie")
                .WithErrorMessage($"De gebouwId '{buildingId}' is niet gekend in het gebouwenregister.");
        }
    }
}
