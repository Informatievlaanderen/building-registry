namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.DataStructures;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : SnapshotBasedTest
    {
        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void ThenBuildingUnitWasAdded()
        {
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId);
            var expectedBuildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(new Fact[]
                    {
                        new Fact(buildingId, expectedBuildingUnitWasAdded),
                        new Fact(buildingId, command.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(buildingId),
                            BuildingSnapshotBuilder
                                .CreateDefaultSnapshot(buildingId)
                                .WithLastModificationFromCrab(Modification.Insert)
                                .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                    new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>{{command.TerrainObjectHouseNumberId, command.HouseNumberId}})
                                .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{command.TerrainObjectHouseNumberId})
                                .WithBuildingUnitCollection(
                                    BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                        .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                        {
                                            BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(expectedBuildingUnitWasAdded)
                                        }))
                                .Build(2, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WhenSameBuildingUnit()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            Fixture.Customize(new WithNoDeleteModification());

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded)
                .When(command)
                .Then(new Fact[]
                {
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(
                                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>{{command.TerrainObjectHouseNumberId, command.HouseNumberId}})
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{command.TerrainObjectHouseNumberId})
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    }))
                            .Build(2, EventSerializerSettings))
                }));
        }
    }
}
