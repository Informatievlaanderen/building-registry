namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObject
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.DataStructures;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnits : SnapshotBasedTest
    {
        public GivenBuildingWithBuildingUnits(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            Fixture.Customize(new WithSnapshotInterval(1));
        }

        // Check if retire, retires all units
        [Fact]
        public void WithFiniteLifetime()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasAdded1 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId1)
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
                .Then(new[] {
                    new Fact(buildingId, new BuildingWasNotRealized(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { Fixture.Create<BuildingUnitId>(), buildingUnitId1 })),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(BuildingStatus.NotRealized)
                        .WithBuildingUnitCollection(
                            BuildingUnitCollectionSnapshotBuilder
                                .CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding(),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding(),
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding()
                                })
                            )
                        .Build(6, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrection()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasAdded1 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId1)
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
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasCorrectedToNotRealized(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { Fixture.Create<BuildingUnitId>(), buildingUnitId1 })),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatus(BuildingStatus.NotRealized)
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder
                                    .CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded).WithStatus(BuildingUnitStatus.NotRealized).WithRetiredByBuilding()
                                    })
                            )
                            .Build(6, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithFiniteLifetimeWhenRealized()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasAdded1 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId1)
                .WithBuildingUnitKey(buildingUnitKey1);

            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId)
                .WithBuildingUnitKey(commonBuildingUnitKey);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>(),
                    buildingUnitWasAdded,
                    buildingUnitWasAdded1,
                    commonBuildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasRetired(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { Fixture.Create<BuildingUnitId>(), buildingUnitId1 })),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatus(BuildingStatus.Retired)
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder
                                    .CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding()
                                    })
                            )
                            .Build(7, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrectionWhenRealized()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasAdded1 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId1)
                .WithBuildingUnitKey(buildingUnitKey1);

            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId)
                .WithBuildingUnitKey(commonBuildingUnitKey);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>(),
                    buildingUnitWasAdded,
                    buildingUnitWasAdded1,
                    commonBuildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasCorrectedToRetired(buildingId, new List<BuildingUnitId> {commonBuildingUnitId}, new List<BuildingUnitId> {Fixture.Create<BuildingUnitId>(), buildingUnitId1})),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatus(BuildingStatus.Retired)
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder
                                    .CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded).WithStatus(BuildingUnitStatus.Retired).WithRetiredByBuilding()
                                    })
                            )
                            .Build(7, EventSerializerSettings))
                }));
        }

        // Check if unretire building, creates new units


        // Check if remove building, removes all units, incl historized units
        [Fact]
        public void WithModificationDelete()
        {
            var importTerrainObjectHouseNumber = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var buildingUnitId2 = BuildingUnitId.Create(buildingUnitKey1, 2);
            var buildingUnitId3 = BuildingUnitId.Create(buildingUnitKey1, 3);

            var commonBuildingUnitKey = new BuildingUnitKey(Fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);
            var commonBuildingUnitId2 = BuildingUnitId.Create(commonBuildingUnitKey, 2);
            var commonBuildingUnitId3 = BuildingUnitId.Create(commonBuildingUnitKey, 3);

            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();
            var buildingUnitWasAdded1 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId1)
                .WithBuildingUnitKey(buildingUnitKey1);

            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId)
                .WithBuildingUnitKey(commonBuildingUnitKey);

            var buildingUnitWasAdded2 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId2)
                .WithBuildingUnitKey(buildingUnitKey1);
            var commonBuildingUnitWasAdded2 = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId2)
                .WithBuildingUnitKey(commonBuildingUnitKey);

            var buildingUnitWasAdded3 = Fixture.Create<BuildingUnitWasAdded>()
                .WithBuildingUnitId(buildingUnitId3)
                .WithBuildingUnitKey(buildingUnitKey1);

            var commonBuildingUnitWasAdded3 = Fixture.Create<CommonBuildingUnitWasAdded>()
                .WithBuildingUnitId(commonBuildingUnitId3)
                .WithBuildingUnitKey(commonBuildingUnitKey);
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    buildingUnitWasAdded1,
                    commonBuildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId),

                    Fixture.Create<BuildingUnitWasNotRealized>()
                        .WithBuildingUnitId(buildingUnitId1),
                    Fixture.Create<BuildingUnitWasRetired>()
                        .WithBuildingUnitId(commonBuildingUnitId),

                    buildingUnitWasAdded2,
                    commonBuildingUnitWasAdded2,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId2),

                    Fixture.Create<BuildingUnitWasNotRealized>()
                        .WithBuildingUnitId(buildingUnitId2),
                    Fixture.Create<BuildingUnitWasRetired>()
                        .WithBuildingUnitId(commonBuildingUnitId2),

                    buildingUnitWasAdded3,
                    commonBuildingUnitWasAdded3,
                    Fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId3)
                )
                .When(importTerrainObjectHouseNumber)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasRemoved(buildingId, new[] { buildingUnitId1, buildingUnitId2, commonBuildingUnitId, commonBuildingUnitId2, Fixture.Create<BuildingUnitId>(), buildingUnitId3, commonBuildingUnitId3 })),
                    new Fact(buildingId, importTerrainObjectHouseNumber.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithIsRemoved(true)
                            .WithLastModificationFromCrab(Modification.Delete)
                            .WithBuildingUnitCollection(
                                BuildingUnitCollectionSnapshotBuilder
                                    .CreateDefaultSnapshot()
                                    .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                    {
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded1).WithStatus(BuildingUnitStatus.NotRealized).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded).WithStatus(BuildingUnitStatus.Retired).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded2).WithStatus(BuildingUnitStatus.NotRealized).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded2).WithStatus(BuildingUnitStatus.Retired).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded3).WithRemoved(),
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded3).WithStatus(BuildingUnitStatus.Realized).WithRemoved(),
                                    })
                            )
                            .Build(16, EventSerializerSettings))
                }));

        }
    }
}
