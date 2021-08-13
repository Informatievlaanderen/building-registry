namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : SnapshotBasedTest
    {
        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        // Check if adding building unit to retired building, building unit is retired
        [Fact]
        public void WithInfiniteLifetimeThenRetireNewlyAddedBuildingUnit()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            Fixture.Customize(new WithInfiniteLifetime());
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            var expectedUnitWasAddedToRetiredBuilding = new BuildingUnitWasAddedToRetiredBuilding(buildingId, Fixture.Create<BuildingUnitId>(), Fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new Fact[]
                    {
                        new Fact(buildingId, expectedUnitWasAddedToRetiredBuilding),
                        new Fact(buildingId, command.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(buildingId),
                            BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                                .WithStatus(BuildingStatus.NotRealized)
                                .WithBuildingUnitCollection(
                                    BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot().WithBuildingUnits(new List<BuildingUnitSnapshot>{ BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(expectedUnitWasAddedToRetiredBuilding, BuildingUnitStatus.Retired) }))
                                .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                                {
                                    { command.TerrainObjectHouseNumberId, command.HouseNumberId }
                                })
                                .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{command.TerrainObjectHouseNumberId})
                                .Build(3, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WithFiniteLifetimeThenRetireNewlyAddedBuildingUnit()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAddedToRetiredBuilding(buildingId, Fixture.Create<BuildingUnitId>(), Fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    command.ToLegacyEvent()));
        }
    }
}
