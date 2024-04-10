namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentValidation.TestHelper;
    using Moq;
    using SqlStreamStore;
    using System.Threading.Tasks;
    using Xunit;

    public class MoveBuildingUnitRequestValidatorTests
    {
        private readonly MoveBuildingUnitRequestValidator _validator;

        public MoveBuildingUnitRequestValidatorTests()
        {
            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamNotFound();
            _validator = new MoveBuildingUnitRequestValidator(new BuildingExistsValidator(streamStoreMock.Object));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("http://bla/a")]
        public async Task GivenInvalidDestinationBuildingId_ThenReturnsExpectedFailure(string doelgebouwId)
        {
            var result = await _validator.TestValidateAsync(new MoveBuildingUnitRequest
            {
                DoelgebouwId = doelgebouwId
            });

            result.ShouldHaveValidationErrorFor(nameof(MoveBuildingUnitRequest.DoelgebouwId))
                .WithErrorCode("GebouwIdOngeldig")
                .WithErrorMessage("Ongeldig gebouwId.");
        }

        [Theory]
        [InlineData("http://bla/1")]
        public async Task GivenNonExistingDestinationBuildingId_ThenReturnsExpectedFailure(string doelgebouwId)
        {
            var result = await _validator.TestValidateAsync(new MoveBuildingUnitRequest
            {
                DoelgebouwId = doelgebouwId
            });

            result.ShouldHaveValidationErrorFor(nameof(MoveBuildingUnitRequest.DoelgebouwId))
                .WithErrorCode("GebouwIdNietGekendValidatie")
                .WithErrorMessage($"Het gebouwId '{doelgebouwId}' is niet gekend in het gebouwenregister.");
        }
    }
}
