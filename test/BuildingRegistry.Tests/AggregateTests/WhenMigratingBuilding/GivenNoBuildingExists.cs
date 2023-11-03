namespace BuildingRegistry.Tests.AggregateTests.WhenMigratingBuilding
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
    using BuildingRegistry.Legacy;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using Building = Building.Building;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = BuildingRegistry.Legacy.ExtendedWkbGeometry;

    public class GivenNoBuildingExists : BuildingRegistryTest
    {
        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void ThenBuildingWasMigratedEvent()
        {
            var command = Fixture.Create<MigrateBuilding>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasMigrated(
                        command.BuildingId,
                        command.BuildingPersistentLocalId,
                        command.BuildingPersistentLocalIdAssignmentDate,
                        command.BuildingStatus,
                        command.BuildingGeometry,
                        command.IsRemoved,
                        command.BuildingUnits
                    ))));
        }

        [Fact]
        public void ThenBuildingWasCorrectlyMutated()
        {
            var command = Fixture.Create<MigrateBuilding>();

            // Act
            var result = Building.MigrateBuilding(
                new BuildingFactory(NoSnapshotStrategy.Instance),
                command.BuildingId,
                command.BuildingPersistentLocalId,
                command.BuildingPersistentLocalIdAssignmentDate,
                command.BuildingStatus,
                command.BuildingGeometry,
                command.IsRemoved,
                command.BuildingUnits);

            // Assert
            result.Should().NotBeNull();
            result.BuildingPersistentLocalId.Should().Be(command.BuildingPersistentLocalId);
            result.BuildingStatus.Should().Be(command.BuildingStatus);
            result.BuildingGeometry.Should().Be(command.BuildingGeometry);
            result.IsRemoved.Should().Be(command.IsRemoved);

            foreach (var expectedBuildingUnit in command.BuildingUnits)
            {
                var actualBuildingUnit = result.BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == expectedBuildingUnit.BuildingUnitPersistentLocalId);
                actualBuildingUnit.Should().NotBeNull();
                actualBuildingUnit!.BuildingUnitPersistentLocalId.Should().Be(expectedBuildingUnit.BuildingUnitPersistentLocalId);
                actualBuildingUnit.Function.Should().Be(expectedBuildingUnit.Function);
                actualBuildingUnit.Status.Should().Be(expectedBuildingUnit.Status);
                actualBuildingUnit.AddressPersistentLocalIds.Should().BeEquivalentTo(expectedBuildingUnit.AddressPersistentLocalIds);
                actualBuildingUnit.IsRemoved.Should().Be(expectedBuildingUnit.IsRemoved);
                actualBuildingUnit.BuildingUnitPosition.Should().Be(expectedBuildingUnit.BuildingUnitPosition);
            }
        }

        [Fact]
        public void WithBuildingUnitOutsideBuilding_ThenBuildingWasCorrectlyMutated()
        {
            var command = Fixture.Create<MigrateBuilding>();
            var buildingUnitWithInvalidPositionToMigrate = new BuildingUnitBuilder(Fixture)
                .WithPosition(new BuildingUnitPosition(
                    new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                    BuildingUnitPositionGeometryMethod.DerivedFromObject))
                .Build();

            command.BuildingUnits.Clear();
            command.BuildingUnits.Add(buildingUnitWithInvalidPositionToMigrate);

            // Act
            var result = Building.MigrateBuilding(
                new BuildingFactory(NoSnapshotStrategy.Instance),
                command.BuildingId,
                command.BuildingPersistentLocalId,
                command.BuildingPersistentLocalIdAssignmentDate,
                command.BuildingStatus,
                command.BuildingGeometry,
                command.IsRemoved,
                command.BuildingUnits);

            // Assert
            result.Should().NotBeNull();
            result.BuildingPersistentLocalId.Should().Be(command.BuildingPersistentLocalId);
            result.BuildingStatus.Should().Be(command.BuildingStatus);
            result.BuildingGeometry.Should().Be(command.BuildingGeometry);
            result.IsRemoved.Should().Be(command.IsRemoved);

            var actualBuildingUnit = result.BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitWithInvalidPositionToMigrate.BuildingUnitPersistentLocalId);
            actualBuildingUnit.Should().NotBeNull();
            actualBuildingUnit!.BuildingUnitPersistentLocalId.Should().Be(buildingUnitWithInvalidPositionToMigrate.BuildingUnitPersistentLocalId);
            actualBuildingUnit.Function.Should().Be(buildingUnitWithInvalidPositionToMigrate.Function);
            actualBuildingUnit.Status.Should().Be(buildingUnitWithInvalidPositionToMigrate.Status);
            actualBuildingUnit.AddressPersistentLocalIds.Should().BeEquivalentTo(buildingUnitWithInvalidPositionToMigrate.AddressPersistentLocalIds);
            actualBuildingUnit.IsRemoved.Should().BeFalse();
            actualBuildingUnit.BuildingUnitPosition.Should().Be(buildingUnitWithInvalidPositionToMigrate.BuildingUnitPosition);
        }
    }
}
