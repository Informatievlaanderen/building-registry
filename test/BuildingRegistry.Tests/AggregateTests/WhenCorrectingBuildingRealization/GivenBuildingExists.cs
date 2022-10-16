namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingRealization
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
    using Extensions;
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
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusRealized_ThenBuildingWasCorrectToUnderConstruction()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasCorrectedFromRealizedToUnderConstruction(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithRealizedBuildingUnits_ThenBuildingUnitsWereCorrectedToPlanned()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var firstBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var secondBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var thirdBuildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>().WithBuildingUnitStatus(BuildingUnitStatus.Planned);
            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    firstBuildingUnitWasPlannedV2,
                    secondBuildingUnitWasPlannedV2,
                    commonBuildingUnitWasAddedV2,
                    thirdBuildingUnitWasPlannedV2,
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)),
                    Fixture.Create<BuildingUnitWasRealizedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasCorrectedFromRealizedToUnderConstruction(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusUnderConstruction_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WhenBuildingIsRemoved_ThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

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

        [Theory]
        [InlineData("Planned")]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

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
        public void WhenBuildingGeometryMethodIsMeasuredByGrb_ThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.MeasuredByGrb),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidBuildingGeometryMethodException()));
        }

        [Fact]
        public void WhenBuildingHasRetiredBuildingUnits_ThrowsBuildingHasRetiredBuildingUnitsException()
        {
            var command = Fixture.Create<CorrectBuildingRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingUnitId>(),
                        Fixture.Create<PersistentLocalId>(),
                        Fixture.Create<BuildingUnitFunction>(),
                        BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        new BuildingRegistry.Legacy.BuildingGeometry(
                            Fixture.Create<BuildingRegistry.Legacy.ExtendedWkbGeometry>(),
                            BuildingRegistry.Legacy.BuildingGeometryMethod.Outlined),
                        false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasRetiredBuildingUnitsException()));
        }

        [Fact]
        public void StateCheck()
        {
            var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(1);
            var realizedBuildingUnitPersistentLocalId = new PersistentLocalId(2);
            var notRealizedBuildingUnitPersistentLocalId = new PersistentLocalId(3);
            var removedBuildingUnitPersistentLocalId = new PersistentLocalId(4);

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        plannedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        realizedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        notRealizedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        false),
                    new BuildingUnit(
                        new BuildingUnitId(Guid.NewGuid()),
                        removedBuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
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
            sut.CorrectRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.UnderConstruction);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));
            plannedUnit.Status.Should().Be(BuildingUnitStatus.Planned);

            var previouslyRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(realizedBuildingUnitPersistentLocalId));
            previouslyRealizedUnit.Status.Should().Be(BuildingUnitStatus.Planned);

            var notRealizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(notRealizedBuildingUnitPersistentLocalId));
            notRealizedUnit.Status.Should().Be(BuildingUnitStatus.NotRealized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));
            removedUnit.Status.Should().Be(BuildingUnitStatus.Realized);
        }
    }
}
