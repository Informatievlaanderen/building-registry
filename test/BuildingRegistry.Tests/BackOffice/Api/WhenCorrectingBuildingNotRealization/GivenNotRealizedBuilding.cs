namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingNotRealization
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

    public class GivenNotRealizedBuilding : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenNotRealizedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenShouldSucceed()
        {
            const string expectedHash = "123456";

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingNotRealizationRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, expectedHash));

            var request = new CorrectBuildingNotRealizationRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.CorrectNotRealization(
                ResponseOptions,
                MockValidRequestValidator<CorrectBuildingNotRealizationRequest>(),
                null,
                MockIfMatchValidator(true),
                request,
                null,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<CorrectBuildingNotRealizationRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingDetailUrl, buildingPersistentLocalId));
            result.ETag.Should().Be(expectedHash);
        }
    }
}
