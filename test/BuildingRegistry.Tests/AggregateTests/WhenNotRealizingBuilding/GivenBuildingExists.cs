namespace BuildingRegistry.Tests.AggregateTests.WhenNotRealizingBuilding
{
    using System;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusPlanned_ThenBuildingWasNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasNotRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusNotRealized_ThenDoNothing()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasNotRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Realized")]
        public void WithInvalidStatus_ThrowsBuildingCannotBeRealizedException(string status)
        {
            var command = Fixture.Create<NotRealizeBuilding>();

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
                .Throws(new BuildingStatusPreventsNotRealizeBuildingException()));
        }

        [Fact]
        public void BuildingIsRemoved_ThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

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
            var plannedBuildingUnitPersistentLocalId = new BuildingRegistry.Legacy.PersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new BuildingRegistry.Legacy.PersistentLocalId(456);
            var realizedBuildingUnitPersistentLocalId = new BuildingRegistry.Legacy.PersistentLocalId(789);
            var removedBuildingUnitPersistentLocalId = new BuildingRegistry.Legacy.PersistentLocalId(101);

            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                buildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.UnderConstruction,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>()
                {
                    new BuildingUnit(
                        new BuildingRegistry.Legacy.BuildingUnitId(Guid.NewGuid()),
                        plannedBuildingUnitPersistentLocalId,
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        false),
                    new BuildingUnit(
                        new BuildingRegistry.Legacy.BuildingUnitId(Guid.NewGuid()),
                        retiredBuildingUnitPersistentLocalId,
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        false),
                    new BuildingUnit(
                        new BuildingRegistry.Legacy.BuildingUnitId(Guid.NewGuid()),
                        realizedBuildingUnitPersistentLocalId,
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        false),
                    new BuildingUnit(
                        new BuildingRegistry.Legacy.BuildingUnitId(Guid.NewGuid()),
                        removedBuildingUnitPersistentLocalId,
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        true)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.NotRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.NotRealized);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));
            plannedUnit.Status.Should().Be(BuildingUnitStatus.NotRealized);

            var retiredUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));
            retiredUnit.Status.Should().Be(BuildingUnitStatus.Retired);

            var realizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(realizedBuildingUnitPersistentLocalId));
            realizedUnit.Status.Should().Be(BuildingUnitStatus.Realized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));
            removedUnit.Status.Should().Be(BuildingUnitStatus.Planned);
        }
    }
}
