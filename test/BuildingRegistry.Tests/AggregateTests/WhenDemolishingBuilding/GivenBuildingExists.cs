namespace BuildingRegistry.Tests.AggregateTests.WhenDemolishingBuilding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Datastructures;
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Legacy;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

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
        public void WithGeometryMethodOutline_ThrowsException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(BuildingStatus.Realized),
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined),
                false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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
            var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedV2(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(101));
            ((ISetProvenance)buildingUnitAddressWasAttached).SetProvenance(Fixture.Create<Provenance>());

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

            var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedV2(
                new BuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(101));
            ((ISetProvenance)buildingUnitAddressWasAttached).SetProvenance(Fixture.Create<Provenance>());

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

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(BuildingStatus.Retired),
                Fixture.Create<BuildingGeometry>(),
                false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithBuildingStatusPlanned_ThrowsException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                new BuildingGeometry(Fixture.Create<ExtendedWkbGeometry>(), BuildingGeometryMethod.MeasuredByGrb),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void BuildingIsRemoved_ThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<DemolishBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: true,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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
            var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new PersistentLocalId(456);
            var notRealizedBuildingUnitPersistentLocalId = new PersistentLocalId(789);
            var removedBuildingUnitPersistentLocalId = new PersistentLocalId(101);

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                buildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.UnderConstruction,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        plannedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        retiredBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.Retired,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        notRealizedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.NotRealized,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        removedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        true)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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
