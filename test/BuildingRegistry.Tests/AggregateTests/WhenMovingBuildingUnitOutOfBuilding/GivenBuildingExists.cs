namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitOutOfBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Tests.Extensions;
    using FluentAssertions;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenBuildingUnitMovedOutOfBuilding()
        {
            var command = Fixture.Create<MoveBuildingUnitOutOfBuilding>();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                        ),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        )
                )
                .When(command)
                .Then(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                    new BuildingUnitWasMovedOutOfBuilding(
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                ))
            );
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitWasMovedOutOfBuilding = Fixture.Create<BuildingUnitWasMovedOutOfBuilding>();
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>()
                .WithBuildingPersistentLocalId(buildingUnitWasMovedOutOfBuilding.BuildingPersistentLocalId);
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingPersistentLocalId(buildingUnitWasMovedOutOfBuilding.BuildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasMovedOutOfBuilding.BuildingUnitPersistentLocalId);

            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                buildingUnitWasMovedOutOfBuilding
            });

            // Assert
            sut.BuildingUnits.Should().BeEmpty();
        }
    }
}
