namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.GrbAnoApi;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.OrWegwijsApi;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenNotifyingOutlinedRealizedBuilding : BackOfficeLambdaTest
    {
        public WhenNotifyingOutlinedRealizedBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task GivenOvoCode_ThenNotificationIsSentWithOrganisationName()
        {
            var organisation = "OVO002949";
            var expectedOrganisationName = "agentschap Digitaal Vlaanderen";

            var wegwijsApiProxy = new Mock<IWegwijsApiProxy>();
            wegwijsApiProxy.Setup(x => x.GetOrganisationName(organisation)).ReturnsAsync(expectedOrganisationName);
            var anoApiProxy = new Mock<IAnoApiProxy>();

            // Arrange
            var handler = new NotifyRealizedOutlinedBuildingLambdaHandler(
                wegwijsApiProxy.Object,
                anoApiProxy.Object);

            //Act
            var buildingPersistentLocalId = 100000;
            var dateTimeStatusChange = DateTimeOffset.UtcNow;
            var extendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());

            await handler.Handle(
                new NotifyOutlinedRealizedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    organisation,
                    dateTimeStatusChange,
                    extendedWkbGeometry),
                CancellationToken.None);

            //Assert
            anoApiProxy.Verify(x => x.CreateAnomaly(
                buildingPersistentLocalId,
                dateTimeStatusChange,
                expectedOrganisationName,
                extendedWkbGeometry,
                It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task GivenKboNumberGeoIT_ThenNotificationIsSentWithOrganisationName()
        {
            var organisation = "0867.526.230";
            var expectedOrganisationName = NotifyRealizedOutlinedBuildingLambdaHandler.GeoIt;

            var wegwijsApiProxy = new Mock<IWegwijsApiProxy>();
            wegwijsApiProxy.Setup(x => x.GetOrganisationName(organisation)).ReturnsAsync(expectedOrganisationName);
            var anoApiProxy = new Mock<IAnoApiProxy>();

            // Arrange
            var handler = new NotifyRealizedOutlinedBuildingLambdaHandler(
                wegwijsApiProxy.Object,
                anoApiProxy.Object);

            //Act
            var buildingPersistentLocalId = 100000;
            var dateTimeStatusChange = DateTimeOffset.UtcNow;
            var extendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());

            await handler.Handle(
                new NotifyOutlinedRealizedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    organisation,
                    dateTimeStatusChange,
                    extendedWkbGeometry),
                CancellationToken.None);

            //Assert
            anoApiProxy.Verify(x => x.CreateAnomaly(
                buildingPersistentLocalId,
                dateTimeStatusChange,
                expectedOrganisationName,
                extendedWkbGeometry,
                It.IsAny<CancellationToken>()));
        }
    }
}
