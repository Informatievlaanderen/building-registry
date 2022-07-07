namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithNoPosition_ThenWasPlannedWithBuildingCentroidAsPosition()
        {
            var command = Fixture.Create<PlanBuildingUnit>().WithoutPosition().WithDeviation(false);

            var @buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    @buildingWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        buildingGeometry.Center,
                        command.Function,
                        false))));
        }

        [Fact]
        public void ThenBuildingUnitWasPlanned()
        {
            var command = Fixture.Create<PlanBuildingUnit>().WithDeviation(false);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        command.Position,
                        command.Function,
                        hasDeviation: false))));
        }

        [Fact]
        public void ThenStateWasCorrectlySet()
        {
            var command = Fixture.Create<PlanBuildingUnit>().WithDeviation(false);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var buildingWasPlannedV2 = Fixture.Create<BuildingWasPlannedV2>();
            ((ISetProvenance)buildingWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            building.Initialize(new object[]
            {
                buildingWasPlannedV2,
                buildingUnitWasPlannedV2
            });

            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);
            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnit.BuildingUnitPosition.Geometry.ToString().Should().Be(command.Position.ToString());
            buildingUnit.BuildingUnitPosition.GeometryMethod.ToString().Should().Be(command.PositionGeometryMethod.ToString());
            buildingUnit.Function.Should().Be(command.Function);
            buildingUnit.HasDeviation.Should().BeFalse();
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.LastEventHash.Should().Be(building.LastEventHash);
        }
    }
}
