namespace BuildingRegistry.Tests.AggregateTests.WhenMigratingBuilding
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BuildingRegistryTest
    {
        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

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
                new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()),
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
                actualBuildingUnit.BuildingUnitPersistentLocalId.Should().Be(expectedBuildingUnit.BuildingUnitPersistentLocalId);
                actualBuildingUnit.Function.Should().Be(expectedBuildingUnit.Function);
                actualBuildingUnit.Status.Should().Be(expectedBuildingUnit.Status);
                actualBuildingUnit.AddressPersistentLocalIds.Should().BeEquivalentTo(expectedBuildingUnit.AddressPersistentLocalIds);
                actualBuildingUnit.IsRemoved.Should().Be(expectedBuildingUnit.IsRemoved);
                actualBuildingUnit.BuildingUnitPosition.Should().Be(expectedBuildingUnit.BuildingUnitPosition);
            }
        }
    }
}
