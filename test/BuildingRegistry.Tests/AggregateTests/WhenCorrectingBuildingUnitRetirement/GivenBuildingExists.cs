namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRetirement
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
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithRetiredBuildingUnit_ThenBuildingUnitWasCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            ((ISetProvenance)buildingUnitWasPlanned).SetProvenance(Fixture.Create<Provenance>());

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var validPointInPolygon = new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: validPointInPolygon)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasCorrectedFromRetiredToRealized(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithRealizedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("NotRealized")]
        public void WithInvalidBuildingUnitStatus_ThenThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

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
                        Fixture.Create<BuildingUnitId>(),
                        Fixture.Create<PersistentLocalId>(),
                        BuildingUnitFunction.Unknown,
                        BuildingUnitStatus.Parse(status) ?? throw new ArgumentException(),
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingUnitPosition>(),
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

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                    .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                    .WithBuildingStatus(BuildingStatus.Realized)
                    .WithBuildingUnit(
                        BuildingUnitStatus.Realized,
                        command.BuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Common)
                    .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void NonExistentBuildingUnit_ThenThrowsBuildingUnitNotFoundException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsNotFoundException()));
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Fact]
        public void StateCheck()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var validPointInBuildingGeometry =
                new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());

            var migrateScenario = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    command.BuildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: validPointInBuildingGeometry)
                .Build();

            var eventToTest = new BuildingUnitWasCorrectedFromRetiredToRealized(
                command.BuildingPersistentLocalId,
                command.BuildingUnitPersistentLocalId);
            ((ISetProvenance)eventToTest).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new object[]
            {
                migrateScenario, eventToTest
            });

            // Assert
            var buildingUnit = building.BuildingUnits.First();

            buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);
            buildingUnit.BuildingUnitPosition.Geometry.Should().Be(new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()));
            buildingUnit.BuildingUnitPosition.GeometryMethod.Should().Be(BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

            buildingUnit.LastEventHash.Should().NotBe(building.LastEventHash);
        }
    }
}
