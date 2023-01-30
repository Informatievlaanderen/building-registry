namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObject
{
    using System.Collections.Generic;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithBuildingUnits : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithBuildingUnits(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        // Check if retire, retires all units
        [Fact]
        public void WithFiniteLifetime()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId1)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingWasNotRealized(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { _fixture.Create<BuildingUnitId>(), buildingUnitId1 }),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrection()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId1)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingWasCorrectedToNotRealized(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { _fixture.Create<BuildingUnitId>(), buildingUnitId1 }),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeWhenRealized()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId1)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingWasRetired(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { _fixture.Create<BuildingUnitId>(), buildingUnitId1 }),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrectionWhenRealized()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId1)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingWasCorrectedToRetired(buildingId, new List<BuildingUnitId> { commonBuildingUnitId }, new List<BuildingUnitId> { _fixture.Create<BuildingUnitId>(), buildingUnitId1 }),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        // Check if unretire building, creates new units


        // Check if remove building, removes all units, incl historized units
        [Fact]
        public void WithModificationDelete()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey1 = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var buildingUnitId1 = BuildingUnitId.Create(buildingUnitKey1, 1);
            var buildingUnitId2 = BuildingUnitId.Create(buildingUnitKey1, 2);
            var buildingUnitId3 = BuildingUnitId.Create(buildingUnitKey1, 3);

            var commonBuildingUnitKey = new BuildingUnitKey(_fixture.Create<BuildingUnitKeyType>());
            var commonBuildingUnitId = BuildingUnitId.Create(commonBuildingUnitKey, 1);
            var commonBuildingUnitId2 = BuildingUnitId.Create(commonBuildingUnitKey, 2);
            var commonBuildingUnitId3 = BuildingUnitId.Create(commonBuildingUnitKey, 3);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId1)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId),

                    _fixture.Create<BuildingUnitWasNotRealized>()
                        .WithBuildingUnitId(buildingUnitId1),
                    _fixture.Create<BuildingUnitWasRetired>()
                        .WithBuildingUnitId(commonBuildingUnitId),

                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId2)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId2)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId2),

                    _fixture.Create<BuildingUnitWasNotRealized>()
                        .WithBuildingUnitId(buildingUnitId2),
                    _fixture.Create<BuildingUnitWasRetired>()
                        .WithBuildingUnitId(commonBuildingUnitId2),

                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithBuildingUnitId(buildingUnitId3)
                        .WithBuildingUnitKey(buildingUnitKey1),
                    _fixture.Create<CommonBuildingUnitWasAdded>()
                        .WithBuildingUnitId(commonBuildingUnitId3)
                        .WithBuildingUnitKey(commonBuildingUnitKey),
                    _fixture.Create<BuildingUnitWasRealized>()
                        .WithBuildingUnitId(commonBuildingUnitId3)
                )
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingWasRemoved(buildingId, new[] { buildingUnitId1, buildingUnitId2, commonBuildingUnitId, commonBuildingUnitId2, _fixture.Create<BuildingUnitId>(),
                        buildingUnitId3, commonBuildingUnitId3 }),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }
    }
}
