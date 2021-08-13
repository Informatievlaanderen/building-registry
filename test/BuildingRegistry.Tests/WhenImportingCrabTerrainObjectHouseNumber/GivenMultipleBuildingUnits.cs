namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenMultipleBuildingUnits : SnapshotBasedTest
    {
        public GivenMultipleBuildingUnits(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            var buildingUnitWasAdded1 = buildingUnitWasAdded
                .WithBuildingUnitId(BuildingUnitId.Create(buildingUnitKey1, 1))
                .WithBuildingUnitKey(buildingUnitKey1);

            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId)
                .WithBuildingUnitKey(commonBuildingUnitKey);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    buildingUnitWasAdded1,
                    commonBuildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(new Fact[] {
                    new Fact(buildingId, new BuildingUnitWasNotRealized(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, new BuildingUnitWasRetired(buildingId, commonBuildingUnitId)),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { importTerrainObjectHouseNumber.TerrainObjectHouseNumberId, importTerrainObjectHouseNumber.HouseNumberId }
                            })
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ importTerrainObjectHouseNumber.TerrainObjectHouseNumberId})
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithStatus(BuildingUnitStatus.NotRealized)
                                        .WithAddressIds(new List<AddressId>())
                                        .WithPreviousAddressId(new AddressId(buildingUnitWasAdded.AddressId)),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                        .WithStatus(BuildingUnitStatus.Retired)
                                }))
                            .Build(8, EventSerializerSettings))
                }));
        }
    }
}
