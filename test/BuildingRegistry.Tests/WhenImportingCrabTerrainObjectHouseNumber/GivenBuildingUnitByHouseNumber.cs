namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.DataStructures;
    using Building.Events.Crab;
    using ValueObjects;
    using WhenImportingCrabHouseNumberStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitByHouseNumber : SnapshotBasedTest
    {
        public GivenBuildingUnitByHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void WithStatusIsRealizedWhenLifetimeIsFinite()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(new Fact[] {
                    new Fact(buildingId, new BuildingUnitWasRetired(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ importTerrainObjectHouseNumber.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { importTerrainObjectHouseNumber.TerrainObjectHouseNumberId, importTerrainObjectHouseNumber.HouseNumberId }
                        })
                        .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                        {
                            { new AddressId(buildingUnitWasAdded.AddressId), new List<AddressHouseNumberStatusWasImportedFromCrab>{importStatus.ToLegacyEvent()} }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder
                                    .CreateDefaultSnapshot(buildingId, new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId), new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey), buildingUnitWasAdded.BuildingUnitVersion)
                                    .WithStatus(BuildingUnitStatus.Retired)
                                    .WithPreviousAddressId(new AddressId(buildingUnitWasAdded.AddressId))
                                    .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() })
                            })).Build(6, EventSerializerSettings)
                    )
                }));
        }

        [Fact]
        public void WithStatusIsRealizedWhenLifetimeIsFiniteAndCorrection()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToRetired(buildingId, Fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsRetiredWhenLifetimeIsFinite()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>().WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRetired>())
                .When(importTerrainObjectHouseNumber)
                //.Then(buildingId,
                //    //new BuildingVersionWasIncreased(buildingId, new Version(1)),
                //    //new BuildingUnitWasAdded(buildingId, expectedUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), BuildingUnitId.Create(buildingUnitKey, 1)),
                //    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(1)),
                //    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(2)),
                //    //new BuildingUnitWasNotRealized(buildingId, expectedUnitId),
                //    //new AddressWasDetached(buildingId, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), expectedUnitId),
                //    importTerrainObjectHouseNumber.ToLegacyEvent()));
                .Then(new Fact[]
                {
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithLastModificationFromCrab(Modification.Insert)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ importTerrainObjectHouseNumber.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { importTerrainObjectHouseNumber.TerrainObjectHouseNumberId, importTerrainObjectHouseNumber.HouseNumberId }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder
                                    .CreateDefaultSnapshot(buildingId, new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId), new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey), buildingUnitWasAdded.BuildingUnitVersion)
                                    .WithStatus(BuildingUnitStatus.Retired)
                                    .WithAddressIds(new List<AddressId>{new AddressId(buildingUnitWasAdded.AddressId)})
                            })).Build(3, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithStatusIsCorrectedToRetiredWhenLifetimeIsFinite()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())),
                    Fixture.Create<BuildingUnitWasCorrectedToRetired>())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    //new BuildingVersionWasIncreased(buildingId, new Version(1)),
                    //new BuildingUnitWasAdded(buildingId, expectedUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), BuildingUnitId.Create(buildingUnitKey, 1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(2)),
                    //new BuildingUnitWasNotRealized(buildingId, expectedUnitId),
                    //new AddressWasDetached(buildingId, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), expectedUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsNotRealizedWhenLifetimeIsFinite()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())),
                    Fixture.Create<BuildingUnitWasNotRealized>())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    //new BuildingVersionWasIncreased(buildingId, new Version(1)),
                    //new BuildingUnitWasAdded(buildingId, expectedUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), BuildingUnitId.Create(buildingUnitKey, 1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(2)),
                    //new BuildingUnitWasNotRealized(buildingId, expectedUnitId),
                    //new AddressWasDetached(buildingId, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), expectedUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsCorrectedNotRealizedWhenLifetimeIsFinite()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())),
            Fixture.Create<BuildingUnitWasCorrectedToNotRealized>())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    //new BuildingVersionWasIncreased(buildingId, new Version(1)),
                    //new BuildingUnitWasAdded(buildingId, expectedUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), BuildingUnitId.Create(buildingUnitKey, 1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(1)),
                    //new BuildingUnitVersionWasIncreased(buildingId, expectedUnitId, new Version(2)),
                    //new BuildingUnitWasNotRealized(buildingId, expectedUnitId),
                    //new AddressWasDetached(buildingId, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), expectedUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusPlannedWhenLifetimeIsFinite()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasPlanned>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(new Fact[] {
                    new Fact(buildingId, new BuildingUnitWasNotRealized(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ importTerrainObjectHouseNumber.TerrainObjectHouseNumberId })
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { importTerrainObjectHouseNumber.TerrainObjectHouseNumberId, importTerrainObjectHouseNumber.HouseNumberId }
                        })
                        .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                        {
                            { new AddressId(buildingUnitWasAdded.AddressId), new List<AddressHouseNumberStatusWasImportedFromCrab>{importStatus.ToLegacyEvent()} }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder
                                    .CreateDefaultSnapshot(buildingId, new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId), new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey), buildingUnitWasAdded.BuildingUnitVersion)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(new AddressId(buildingUnitWasAdded.AddressId))
                                    .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() })
                            })).Build(6, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithStatusPlannedWhenLifetimeIsFiniteAndCorrection()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasPlanned>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToNotRealized(buildingId, Fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), Fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithDelete()
        {
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitId = new BuildingUnitId(Guid.NewGuid());
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, buildingUnitId, BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded
                        .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())))
                .When(command)
                .Then(new Fact[]
                    {
                        new Fact(buildingId, new BuildingUnitWasRemoved(buildingId, buildingUnitId)),
                        new Fact(buildingId, command.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{ command.TerrainObjectHouseNumberId })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder
                                        .CreateDefaultSnapshot(buildingId, new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId), new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey), buildingUnitWasAdded.BuildingUnitVersion)
                                        .WithAddressIds(new List<AddressId>{AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())})
                                        .WithRemoved()
                                })).Build(3, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WhenNewBuildingUnit()
        {
            var terrainObjectHouseNumberId = Fixture.Create<int>();
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var commonBuildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId);
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                            .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp)),
                    new CommonBuildingUnitWasAdded(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1), commonBuildingUnitKey, new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp)),
                    new BuildingUnitWasRealized(buildingId, BuildingUnitId.Create(commonBuildingUnitKey, 1)),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeWhenNewBuildingUnit()
        {
            var terrainObjectHouseNumberId = Fixture.Create<int>();
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                            .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, buildingUnitId, buildingUnitKey, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp)),
                    new BuildingUnitWasNotRealized(buildingId, buildingUnitId),
                    new BuildingUnitAddressWasDetached(buildingId, AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId), buildingUnitId),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithNewHouseNumber()
        {
            var newHouseNumberId = new CrabHouseNumberId(Fixture.Create<int>());
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(newHouseNumberId);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);
            var buildingUnitIdv2 = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>()
                            .WithAddressId(AddressId.CreateFor(Fixture.Create<CrabHouseNumberId>())))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, buildingUnitId),
                    new BuildingUnitWasAdded(buildingId, buildingUnitIdv2, buildingUnitKey, AddressId.CreateFor(newHouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp)),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }
    }
}
