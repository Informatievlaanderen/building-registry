namespace BuildingRegistry.Tests.BackOffice.Api.WhenChangingBuildingOutline
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenOutlinedBuilding : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenOutlinedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenShouldSucceed()
        {
            const string expectedHash = "123456";

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingOutlineRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, expectedHash));

            //Act
            var result = (AcceptedWithETagResult)await _controller.ChangeOutline(
                ResponseOptions,
                MockValidRequestValidator<ChangeBuildingOutlineRequest>(),
                null,
                MockIfMatchValidator(true),
                buildingPersistentLocalId,
                new ChangeBuildingOutlineRequest(),
                null,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<ChangeBuildingOutlineRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingDetailUrl, buildingPersistentLocalId));
            result.ETag.Should().Be(expectedHash);
        }
    }
}
