namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabBuildingGeometry
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsComplete : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingIsComplete(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void ThenBuildingBecameIncompleteWhenModificationIsDelete()
        {
            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithCrabModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())),
                    _fixture.Create<BuildingBecameUnderConstruction>(),
                    _fixture.Create<BuildingBecameComplete>())
                .When(importGeometry)
                .Then(buildingId,
                    new BuildingGeometryWasRemoved(buildingId),
                    new BuildingBecameIncomplete(buildingId),
                    importGeometry.ToLegacyEvent())
            );
        }
    }
}
