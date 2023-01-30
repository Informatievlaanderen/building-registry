namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using System;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NodaTime;
    using WhenImportingCrabHouseNumberStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitByHouseNumber : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingUnitByHouseNumber(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void WithStatusIsRealizedWhenLifetimeIsFinite()
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsRealizedWhenLifetimeIsFiniteAndCorrection()
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _fixture.Create<BuildingUnitWasRealized>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToRetired(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsRetiredWhenLifetimeIsFinite()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())),
                    _fixture.Create<BuildingUnitWasRetired>())
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
        public void WithStatusIsCorrectedToRetiredWhenLifetimeIsFinite()
        {
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())),
                    _fixture.Create<BuildingUnitWasCorrectedToRetired>())
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
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())),
                    _fixture.Create<BuildingUnitWasNotRealized>())
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
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var expectedUnitId = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())),
                    _fixture.Create<BuildingUnitWasCorrectedToNotRealized>())
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
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _fixture.Create<BuildingUnitWasPlanned>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusPlannedWhenLifetimeIsFiniteAndCorrection()
        {
            var importStatus = _fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()))
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();
            var buildingUnitWasAdded = _fixture.Create<BuildingUnitWasAdded>()
                .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>()));

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    _fixture.Create<BuildingUnitWasPlanned>(),
                    importStatus.ToLegacyEvent())
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasCorrectedToNotRealized(buildingId, _fixture.Create<BuildingUnitId>()),
                    new BuildingUnitAddressWasDetached(buildingId, new AddressId(buildingUnitWasAdded.AddressId), _fixture.Create<BuildingUnitId>()),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }

        [Fact]
        public void WithDelete()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitId = new BuildingUnitId(Guid.NewGuid());
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, buildingUnitId, BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, buildingUnitId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNewBuildingUnit()
        {
            var terrainObjectHouseNumberId = _fixture.Create<int>();
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var commonBuildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId);
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())))
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
            var terrainObjectHouseNumberId = _fixture.Create<int>();
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId))
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())))
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
            var newHouseNumberId = new CrabHouseNumberId(_fixture.Create<int>());
            var importTerrainObjectHouseNumber = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(newHouseNumberId);

            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(importTerrainObjectHouseNumber.TerrainObjectId, importTerrainObjectHouseNumber.TerrainObjectHouseNumberId);
            var buildingUnitId = BuildingUnitId.Create(buildingUnitKey, 1);
            var buildingUnitIdv2 = BuildingUnitId.Create(buildingUnitKey, 2);

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>()
                        .WithAddressId(AddressId.CreateFor(_fixture.Create<CrabHouseNumberId>())))
                .When(importTerrainObjectHouseNumber)
                .Then(buildingId,
                    new BuildingUnitWasRemoved(buildingId, buildingUnitId),
                    new BuildingUnitWasAdded(buildingId, buildingUnitIdv2, buildingUnitKey, AddressId.CreateFor(newHouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumber.Timestamp)),
                    importTerrainObjectHouseNumber.ToLegacyEvent()));
        }
    }
}
