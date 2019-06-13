namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabHouseNumberStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitBySubaddress : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();
        public GivenBuildingUnitBySubaddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
        }

        //[Fact]
        //public void WithStatusIsRealizedWhenLifetimeIsFinite()
        //{
        //    var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
        //        .WithStatus(CrabAddressStatus.InUse);

        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasRealized>(),
        //            importStatus.ToLegacyEvent())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            new BuildingUnitVersionWasIncreased(buildingId, _fixture.Create<BuildingUnitId>(), new Version(1)),
        //            new BuildingUnitWasRetired(buildingId, _fixture.Create<BuildingUnitId>()),
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusIsRealizedWhenLifetimeIsFiniteAndCorrection()
        //{
        //    var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
        //        .WithStatus(CrabAddressStatus.InUse);

        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
        //        .WithModification(CrabModification.Correction);

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasRealized>(),
        //            importStatus.ToLegacyEvent())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            new BuildingUnitVersionWasIncreased(buildingId, _fixture.Create<BuildingUnitId>(), new Version(1)),
        //            new BuildingUnitWasCorrectedToRetired(buildingId, _fixture.Create<BuildingUnitId>()),
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusIsRetiredWhenLifetimeIsFinite()
        //{
        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasRetired>())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusIsCorrectedToRetiredWhenLifetimeIsFinite()
        //{
        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasCorrectedToRetired>())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusIsNotRealizedWhenLifetimeIsFinite()
        //{
        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasNotRealized>())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusIsCorrectedNotRealizedWhenLifetimeIsFinite()
        //{
        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasCorrectedToNotRealized>())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusPlannedWhenLifetimeIsFinite()
        //{
        //    var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
        //        .WithStatus(CrabAddressStatus.InUse);

        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasPlanned>(),
        //            importStatus.ToLegacyEvent())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            new BuildingUnitVersionWasIncreased(buildingId, _fixture.Create<BuildingUnitId>(), new Version(1)),
        //            new BuildingUnitWasNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WithStatusPlannedWhenLifetimeIsFiniteAndCorrection()
        //{
        //    var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
        //        .WithStatus(CrabAddressStatus.InUse);

        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
        //        .WithModification(CrabModification.Correction);

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>(),
        //            _fixture.Create<BuildingUnitWasPlanned>(),
        //            importStatus.ToLegacyEvent())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            new BuildingUnitVersionWasIncreased(buildingId, _fixture.Create<BuildingUnitId>(), new Version(1)),
        //            new BuildingUnitWasCorrectedToNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}

        //[Fact]
        //public void WhenNewBuildingUnit()
        //{
        //    var terrainObjectHouseNumberId = _fixture.Create<int>();
        //    var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
        //        .WithHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId));

        //    var buildingId = _fixture.Create<BuildingId>();

        //    Assert(new Scenario()
        //        .Given(buildingId,
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingUnitWasAdded>())
        //        .When(importTerrainObjectHouseNumber)
        //        .Then(buildingId,
        //            new BuildingVersionWasIncreased(buildingId, new Version(1)),
        //            new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.Empty), BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId), AddressId.CreateFor(importTerrainObjectHouseNumber.HouseNumberId)),
        //            new BuildingUnitVersionWasIncreased(buildingId, new BuildingUnitId(Guid.Empty), new Version(1)),
        //            new BuildingVersionWasIncreased(buildingId, new Version(2)),
        //            new CommonBuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.Empty), BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId)),
        //            new BuildingUnitVersionWasIncreased(buildingId, new BuildingUnitId(Guid.Empty), new Version(1)),
        //            new BuildingUnitWasRealized(buildingId, new BuildingUnitId(Guid.Empty)),
        //            importTerrainObjectHouseNumber.ToLegacyEvent()));
        //}
    }


}
