namespace BuildingRegistry.Tests.AggregateTests.WhenDemolishingBuilding
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithStatusRealized_ThenBuildingWasDemolished()
        {
            var command = Fixture.Create<DemolishBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithGeometryMethodOutline_ThenThrowsException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidGeometryMethodException()));
        }

        [Fact]
        public void WithPlannedBuildingUnit_ThenBuildingUnitWasNotRealized()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)
                .WithAddressPersistentLocalId(101)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitAddressWasAttached)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasDemolished(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithRealizedBuildingUnit_ThenBuildingUnitWasRetired()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = Fixture.Create<BuildingUnitWasRealizedV2>();

            var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)
                .WithAddressPersistentLocalId(101)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitWasRealizedV2,
                    buildingUnitAddressWasAttached)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredBecauseBuildingWasDemolished(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasRealizedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithRetiredBuildingUnit_ThenOnlyBuildingEvent()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = Fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasRetiredV2 = Fixture.Create<BuildingUnitWasRetiredV2>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitWasRealizedV2,
                    buildingUnitWasRetiredV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithNotRealizedBuildingUnit_ThenOnlyBuildingEvent()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var unplannedBuildingWasRealizedAndMeasured = Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();
            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = Fixture.Create<BuildingUnitWasNotRealizedV2>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    unplannedBuildingWasRealizedAndMeasured,
                    buildingUnitWasPlannedV2,
                    buildingUnitWasNotRealized)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenOneEvent()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = Fixture.Create<BuildingUnitWasRemovedV2>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitWasRemovedV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasDemolished(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId, command.BuildingGrbData))));
        }

        [Fact]
        public void WithStatusRetired_ThenDoNothing()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Retired)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithBuildingStatusPlanned_ThenThrowsException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(new BuildingGeometry(Fixture.Create<ExtendedWkbGeometry>(), BuildingGeometryMethod.MeasuredByGrb))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void BuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void StateCheck()
        {
            var plannedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);
            var notRealizedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(789);
            var removedBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(101);

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(buildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    plannedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    retiredBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    notRealizedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    removedBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    isRemoved: true)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.RealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.Realized);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));
            plannedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            var retiredUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));
            retiredUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Retired);

            var notRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(notRealizedBuildingUnitPersistentLocalId));
            notRealizedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.NotRealized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));
            removedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
        }
    }
}
