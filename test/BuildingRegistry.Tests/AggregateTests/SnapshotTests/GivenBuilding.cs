namespace BuildingRegistry.Tests.AggregateTests.SnapshotTests
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Legacy.Autofixture;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using WhenPlanningBuildingUnit;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = Building.BuildingUnit;

    public class GivenBuilding : BuildingRegistryTest
    {
        private readonly BuildingStreamId _streamId;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithValidPolygon());
            _streamId = new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>());
        }

        [Fact]
        public async Task ThenSnapshotWasCreated()
        {
            Fixture.Register(() => (ISnapshotStrategy)IntervalStrategy.SnapshotEvery(1));

            var provenance = Fixture.Create<Provenance>();
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            var planBuildingUnit = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithProvenance(provenance);

            var expectedEvent = new BuildingUnitWasPlannedV2(
                planBuildingUnit.BuildingPersistentLocalId,
                planBuildingUnit.BuildingUnitPersistentLocalId,
                planBuildingUnit.PositionGeometryMethod,
                buildingGeometry.Center,
                BuildingUnitFunction.Unknown,
                planBuildingUnit.HasDeviation);
            ((ISetProvenance)expectedEvent).SetProvenance(provenance);

            var expectedEvent2 = new CommonBuildingUnitWasAddedV2(
                planBuildingUnit.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                buildingGeometry.Center,
                hasDeviation: false);
            ((ISetProvenance)expectedEvent2).SetProvenance(provenance);

            Assert(new Scenario()
                .Given(_streamId,
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(planBuildingUnit)
                .Then(
                    new Fact(_streamId, expectedEvent),
                    new Fact(_streamId, expectedEvent2)));

            var plannedBuildingUnit = new BuildingUnit(o => { });
            plannedBuildingUnit.Route(buildingUnitWasPlanned);
            var buildingUnit = new BuildingUnit(o => { });
            buildingUnit.Route(expectedEvent);
            var commonBuildingUnit = new BuildingUnit(o => { });
            commonBuildingUnit.Route(expectedEvent2);

            var expectedSnapshot = new BuildingSnapshot(
                Fixture.Create<BuildingPersistentLocalId>(),
                BuildingStatus.Planned,
                buildingGeometry,
                false,
                expectedEvent2.GetHash(),
                expectedEvent2.Provenance,
                new List<BuildingUnit>
                {
                    plannedBuildingUnit,
                    buildingUnit,
                    commonBuildingUnit
                },
                new List<BuildingUnit>());

            var snapshotStore = (ISnapshotStore)Container.Resolve(typeof(ISnapshotStore));
            var latestSnapshot = await snapshotStore.FindLatestSnapshotAsync(_streamId, CancellationToken.None);

            latestSnapshot.Should().NotBeNull();
            var snapshot = JsonConvert.DeserializeObject<BuildingSnapshot>(latestSnapshot!.Data, EventSerializerSettings);

            snapshot.Should().BeEquivalentTo(expectedSnapshot, options =>
            {
                options.Excluding(x => x.Path.EndsWith("LastEventHash"));
                options.Excluding(x => x.Type == typeof(Instant));
                return options;
            });
        }

        [Fact]
        public async Task WithMigratedBuildingAndUnusedCommonBuildingUnits_ThenSnapshotWasCreated()
        {
            Fixture.Register(() =>
            {
                var mock = new Mock<ISnapshotStrategy>();
                mock.Setup(x => x.ShouldCreateSnapshot(It.IsAny<SnapshotStrategyContext>()))
                    .Returns(true);

                return mock.Object;
            });

            var provenance = Fixture.Create<Provenance>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingGeometry = Fixture.Create<BuildingGeometry>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(1),
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                    positionGeometryMethod: BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                    new BuildingUnitPersistentLocalId(3),
                    function: BuildingRegistry.Legacy.BuildingUnitFunction.Common)
                .Build();

            var expectedEvent = Fixture.Create<BuildingBecameUnderConstructionV2>();
            ((ISetProvenance)expectedEvent).SetProvenance(provenance);

            Assert(new Scenario()
                .Given(_streamId, buildingWasMigrated)
                .When(new PlaceBuildingUnderConstruction(Fixture.Create<BuildingPersistentLocalId>(), provenance))
                .Then(new Fact(_streamId, expectedEvent)));

            var plannedBuildingUnit = BuildingUnit.Migrate(
                e => {},
                buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(1),
                BuildingUnitFunction.Unknown,
                BuildingUnitStatus.Planned,
                [],
                new BuildingUnitPosition(
                    buildingGeometry.Center,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject),
                false);
            plannedBuildingUnit.Route(buildingWasMigrated);

            var commonBuildingUnit = BuildingUnit.Migrate(
                e => {},
                buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(2),
                BuildingUnitFunction.Common,
                BuildingUnitStatus.Planned,
                [],
                new BuildingUnitPosition(
                    buildingGeometry.Center,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject),
                false);
            commonBuildingUnit.Route(buildingWasMigrated);

            var unusedCommonBuildingUnit = BuildingUnit.Migrate(
                e => {},
                buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(3),
                BuildingUnitFunction.Common,
                BuildingUnitStatus.NotRealized,
                [],
                new BuildingUnitPosition(
                    buildingGeometry.Center,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject),
                false);
            unusedCommonBuildingUnit.Route(buildingWasMigrated);

            var expectedSnapshot = new BuildingSnapshot(
                buildingPersistentLocalId,
                BuildingStatus.UnderConstruction,
                new BuildingGeometry(
                    new ExtendedWkbGeometry(buildingWasMigrated.ExtendedWkbGeometry),
                    BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod)),
                false,
                expectedEvent.GetHash(),
                expectedEvent.Provenance,
                new List<BuildingUnit>
                {
                    plannedBuildingUnit,
                    commonBuildingUnit
                },
                new List<BuildingUnit>
                {
                    unusedCommonBuildingUnit
                });

            var snapshotStore = (ISnapshotStore)Container.Resolve(typeof(ISnapshotStore));
            var latestSnapshot = await snapshotStore.FindLatestSnapshotAsync(_streamId, CancellationToken.None);

            latestSnapshot.Should().NotBeNull();
            var snapshot = JsonConvert.DeserializeObject<BuildingSnapshot>(latestSnapshot!.Data, EventSerializerSettings);

            snapshot.Should().BeEquivalentTo(expectedSnapshot, options =>
            {
                options.Excluding(x => x.Path.EndsWith("LastEventHash"));
                options.Excluding(x => x.Type == typeof(Instant));
                return options;
            });
        }
    }
}
