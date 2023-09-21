namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System.Threading.Tasks;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenNotifyingOutlinedRealizedBuilding : BackOfficeLambdaTest
    {
        public WhenNotifyingOutlinedRealizedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task ThenBuildingIsRealized()
        {
            // // Arrange
            // var handler = new NotifyRealizedOutlinedBuildingLambdaHandler();
            //
            // //Act
            // await handler.Handle(
            //     new NotifyOutlinedRealizedBuildingLambdaRequest(
            //         DateTimeOffset.Now,
            //         new ExtendedWkbGeometry(""),
            //         "DigitaalVlaanderen"),
            //     CancellationToken.None);
            //
            // //Assert
        }
    }
}
