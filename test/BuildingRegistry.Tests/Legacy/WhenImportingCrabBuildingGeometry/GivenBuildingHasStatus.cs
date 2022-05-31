namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabBuildingGeometry
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingHasStatus : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingHasStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void ThenBuildingBecameIncompleteWhenModificationIsDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            var buildingId = _fixture.Create<BuildingId>();
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingBecameUnderConstruction>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingWasMeasuredByGrb(buildingId, GeometryHelper.CreateEwkbFrom(importStatus.BuildingGeometry)),
                    new BuildingBecameComplete(buildingId),
                    importStatus.ToLegacyEvent())
            );
        }
    }
}
