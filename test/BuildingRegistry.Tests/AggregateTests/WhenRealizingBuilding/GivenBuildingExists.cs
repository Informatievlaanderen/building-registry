namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingBuilding
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
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Legacy;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusUnderConstruction_ThenBuildingWasRealized()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnits_ThenBuildingUnitsWereRealized()
        {
            var command = Fixture.Create<RealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasRealizedV2(command.BuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRealizedBecauseBuildingWasRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithStatusRealized_ThenDoNothing()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithStatusPlanned_ThrowsBuildingHasInvalidStatusException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThrowsBuildingCannotBeRealizedException(string status)
        {
            var command = Fixture.Create<RealizeBuilding>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(status),
                Fixture.Create<BuildingGeometry>(),
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
            var command = Fixture.Create<RealizeBuilding>();

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

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>()).Create();
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
