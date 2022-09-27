namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuilding
{
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Building;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenBuildingIsPlanned()
        {
            const int expectedLocation = 5;
            const string expectedHash = "123456";

            //Arrange
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedLocation);

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None).Result)
                .Returns(new PlanBuildingResponse(expectedLocation, expectedHash));

            var mockPlanBuildingRequestValidator = new Mock<IValidator<PlanBuildingRequest>>();
            mockPlanBuildingRequestValidator
                .Setup(x => x.ValidateAsync(It.IsAny<PlanBuildingRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ValidationResult()));

            //Act
            var result = (AcceptedWithETagResult)await _controller.Plan(
                ResponseOptions,
                mockPlanBuildingRequestValidator.Object,
                new PlanBuildingRequest());

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingDetailUrl, expectedLocation));
            result.ETag.Should().Be(expectedHash);
        }
    }
}
