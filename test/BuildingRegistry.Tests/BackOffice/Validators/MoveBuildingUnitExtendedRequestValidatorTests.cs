namespace BuildingRegistry.Tests.BackOffice.Validators
{
    using System.Threading.Tasks;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentValidation.TestHelper;
    using Moq;
    using SqlStreamStore;
    using Xunit;

    public class MoveBuildingUnitExtendedRequestValidatorTests
    {
        private const int ExistingSourceBuildingPersistentLocalId = 2000000;
        private const int ExistingBuildingUnitPersistentLocalId = 2000001;

        private readonly Mock<IStreamStore> _streamStore;

        private readonly MoveBuildingUnitExtendedRequestValidator _extendedRequestValidator;

        public MoveBuildingUnitExtendedRequestValidatorTests()
        {
            _streamStore = new Mock<IStreamStore>();

            var backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext([]);
            backOfficeContext.AddBuildingUnitBuilding(
                new BuildingPersistentLocalId(ExistingSourceBuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(ExistingBuildingUnitPersistentLocalId)).GetAwaiter().GetResult();

            _extendedRequestValidator = new MoveBuildingUnitExtendedRequestValidator(
                new BuildingExistsValidator(_streamStore.Object),
                backOfficeContext);
        }

        [Fact]
        public async Task GivenValidRequest()
        {
            _streamStore.SetStreamFound();

            var doelgebouwId = $"https://data.vlaanderen.be/id/gebouw/{ExistingSourceBuildingPersistentLocalId + 1}";
            var moveBuildingUnitRequest = new MoveBuildingUnitRequest { DoelgebouwId = doelgebouwId };
            var extendedRequest = new MoveBuildingUnitExtendedRequest(moveBuildingUnitRequest, ExistingBuildingUnitPersistentLocalId);

            var result = await _extendedRequestValidator.TestValidateAsync(extendedRequest);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        [InlineData("http://bla/a")]
        public async Task GivenInvalidDestinationBuildingId_ThenReturnsExpectedFailure(string doelgebouwId)
        {
            var moveBuildingUnitRequest = new MoveBuildingUnitRequest { DoelgebouwId = doelgebouwId };
            var extendedRequest = new MoveBuildingUnitExtendedRequest(moveBuildingUnitRequest, ExistingBuildingUnitPersistentLocalId);
            var result = await _extendedRequestValidator.TestValidateAsync(extendedRequest);

            result.ShouldHaveValidationErrorFor(nameof(MoveBuildingUnitRequest.DoelgebouwId))
                .WithErrorCode("GebouwIdOngeldig")
                .WithErrorMessage("Ongeldig gebouwId.");
        }

        [Theory]
        [InlineData("http://bla/1")]
        public async Task GivenNonExistingDestinationBuildingId_ThenReturnsExpectedFailure(string doelgebouwId)
        {
            _streamStore.SetStreamNotFound();

            var moveBuildingUnitRequest = new MoveBuildingUnitRequest { DoelgebouwId = doelgebouwId };
            var extendedRequest = new MoveBuildingUnitExtendedRequest(moveBuildingUnitRequest, ExistingBuildingUnitPersistentLocalId);
            var result = await _extendedRequestValidator.TestValidateAsync(extendedRequest);

            result.ShouldHaveValidationErrorFor(nameof(MoveBuildingUnitRequest.DoelgebouwId))
                .WithErrorCode("GebouwIdNietGekendValidatie")
                .WithErrorMessage($"Het gebouwId '{doelgebouwId}' is niet gekend in het gebouwenregister.");
        }

        [Fact]
        public async Task GivenSourceBuildingIsTheSameAsDestinationBuilding_ThenReturnsExpectedFailure()
        {
            _streamStore.SetStreamFound();

            var doelgebouwId = $"https://data.vlaanderen.be/id/gebouw/{ExistingSourceBuildingPersistentLocalId}";
            var moveBuildingUnitRequest = new MoveBuildingUnitRequest { DoelgebouwId = doelgebouwId };
            var extendedRequest = new MoveBuildingUnitExtendedRequest(moveBuildingUnitRequest, ExistingBuildingUnitPersistentLocalId);
            var result = await _extendedRequestValidator.TestValidateAsync(extendedRequest);

            result.ShouldHaveValidationErrorFor(nameof(MoveBuildingUnitRequest.DoelgebouwId))
                .WithErrorCode("BrongebouwIdHetzelfdeAlsDoelgebouwId")
                .WithErrorMessage($"Het brongebouwId is hetzelfde als het doelgebouwId: {doelgebouwId}.");
        }
    }
}
