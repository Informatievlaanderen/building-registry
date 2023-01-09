namespace BuildingRegistry.Tests.AggregateTests.WhenNotRealizingBuilding
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
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
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
        public void WithPlannedBuildingUnits_ThenBuildingUnitsWereNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = Fixture.Create<BuildingUnitAddressWasAttachedV2>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitAddressWasAttachedV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
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
        public void WithInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
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
                .Throws(new BuildingHasInvalidStatusException()));
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
            var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new PersistentLocalId(456);
            var realizedBuildingUnitPersistentLocalId = new PersistentLocalId(789);
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
                        new List<AddressPersistentLocalId>  { new AddressPersistentLocalId(5) },
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
                        realizedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.Realized,
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

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.NotRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.NotRealized);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));
            plannedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.NotRealized);
            plannedUnit.AddressPersistentLocalIds.Should().BeEmpty();

            var retiredUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));
            retiredUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Retired);

            var realizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(realizedBuildingUnitPersistentLocalId));
            realizedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));
            removedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
        }
    }
}
