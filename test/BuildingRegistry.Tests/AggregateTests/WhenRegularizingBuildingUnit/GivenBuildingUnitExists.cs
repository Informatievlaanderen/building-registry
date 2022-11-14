namespace BuildingRegistry.Tests.AggregateTests.WhenRegularizingBuildingUnit
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
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingUnitExists: BuildingRegistryTest
    {
        public GivenBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitWasRegularized()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasRegularized(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithRegularizedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasRegularized>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void BuildingUnitIsRemoved_ThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitFunction>(),
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: true)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidBuildingUnitStatus_ThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Parse(status) ?? throw new ArgumentException(),
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void BuildingStatusNotValid_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<RegularizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(status), 
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitFunction>(),
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
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
        public void StateCheck()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>()).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            ((ISetProvenance)buildingWasPlanned).SetProvenance(Fixture.Create<Provenance>());
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            ((ISetProvenance)buildingUnitWasPlanned).SetProvenance(Fixture.Create<Provenance>());
            var buildingUnitWasRegularized = Fixture.Create<BuildingUnitWasRegularized>();
            ((ISetProvenance)buildingUnitWasPlanned).SetProvenance(Fixture.Create<Provenance>());

            // Act
            building.Initialize(new object[]
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                buildingUnitWasRegularized
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);

            var buildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);
            buildingUnit.HasDeviation.Should().BeFalse();
        }
    }
}
