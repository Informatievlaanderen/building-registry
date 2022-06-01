namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabBuildingStatus
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

    public class GivenBuildingIsPositioned : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingIsPositioned(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
        }

        [Fact]
        public void ThenBuildingBecameIncompleteWhenModificationIsDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.InUse);

            var buildingId = _fixture.Create<BuildingId>();
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())))
                .When(importStatus)
                .Then(buildingId,
                    new BuildingWasRealized(buildingId),
                    new BuildingBecameComplete(buildingId),
                    importStatus.ToLegacyEvent())
            );
        }
    }
}
