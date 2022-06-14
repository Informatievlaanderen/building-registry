namespace BuildingRegistry.Tests.Legacy.Cases
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using NodaTime;
    using Tests.Autofixture;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using WhenReaddressingHouseNumber;
    using WhenReaddressingSubaddress;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase1AVariationsWithReaddressing : TestCase1A
    {
        private LocalDate _readdressingBeginDate;
        public CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }
        public CrabHouseNumberId NewHuisNr16Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }

        public AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public BuildingUnitKey NewGebouweenheid4Key => BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId, _.SubaddressNr16Bus2Id);
        public BuildingUnitId NewGebouweenheid4Id => BuildingUnitId.Create(NewGebouweenheid4Key, 1);
        public BuildingUnitId NewGebouweenheid4IdV2 => BuildingUnitId.Create(NewGebouweenheid4Key, 2);

        public BuildingUnitKey NewGebouweenheid1KeyV2 => BuildingUnitKey.Create(_.Gebouw1CrabTerrainObjectId, NewHuisNr16KoppelingId);
        public BuildingUnitId NewGebouweenheid1IdV2 => BuildingUnitId.Create(NewGebouweenheid1KeyV2, 2);
        public BuildingUnitId NewGebouweenheid1IdV1 => BuildingUnitId.Create(NewGebouweenheid1KeyV2, 1);

        public TestCase1AVariationsWithReaddressing(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(169);
            NewHuisNr16Id = new CrabHouseNumberId(171717);
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(17);
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2ReaddressHouseNr()
        {
            var readdressHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithOldHouseNumberId(_.HuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(readdressHouseNumber)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid1Id, _.Address16Id, NewAddress16Id, ReaddressingBeginDate),
                    readdressHouseNumber.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2ReaddressHouseNrTest()
        {
            Assert(BasedOnT2ReaddressHouseNr());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2ReaddressSubaddress()
        {
            var readdressHouseNumber = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithOldTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithOldSubaddressId(_.SubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2ReaddressHouseNr())
                .When(readdressHouseNumber)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2Id, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    readdressHouseNumber.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2ReaddressSubaddressTest()
        {
            Assert(BasedOnT2ReaddressSubaddress());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2HistorizeHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1)))); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2ReaddressSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2HistorizeHouseNumberTest()
        {
            Assert(BasedOnT2HistorizeHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2HistorizedHouseNumberImportSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2HistorizeHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid4Id, NewGebouweenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, NewGebouweenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, NewGebouweenheid4Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2HistorizeHouseNumberThenImportSubaddressShouldBeHistorized()
        {
            Assert(BasedOnT2HistorizedHouseNumberImportSubaddress());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2HistorizedHouseNumberWithOldTerrainObjectHouseNr()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithHouseNumberId(_.HuisNr16Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2HistorizeHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2HistorizedHouseNumberWithOldTerrainObjectHouseNrTest()
        {
            Assert(BasedOnT2HistorizedHouseNumberWithOldTerrainObjectHouseNr());
        }

        [Fact]
        public void UnretireHouseNumberWithOneSubaddressTest()
        {
            Assert(UnretireHouseNumberWithOneSubaddress());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWithOneSubaddress()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2HistorizeHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV2, NewGebouweenheid1KeyV2, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder T3BasedOnT2ReaddressSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2ReaddressSubaddress())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid4Id, NewGebouweenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void T3BasedOnT2ReaddressSubaddressTest()
        {
            Assert(T3BasedOnT2ReaddressSubaddress());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3HistorizeHouseNumberSpecification()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id) //koppel huisnr 16
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(T3BasedOnT2ReaddressSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, NewGebouweenheid4Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, NewGebouweenheid4Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3HistorizeHouseNumber()
        {
            Assert(BasedOnT3HistorizeHouseNumberSpecification());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressHistorizedByHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3HistorizedHouseNumberImportSubaddress()
        {
            Assert(ImportSubaddressHistorizedByHouseNumber());
        }

        [Fact]
        public void UnretireHouseNumberWithTwoSubaddressesTest()
        {
            Assert(UnretireHouseNumberWithTwoSubaddresses());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWithTwoSubaddresses()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV2, NewGebouweenheid1KeyV2, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid4IdV2, NewGebouweenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), NewGebouweenheid4Id),

                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),

                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),

                    new BuildingUnitWasNotRealized(_.Gebouw1Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void RetireSubaddressAlreadyRetiredByHouseNumber()
        {
            Assert(T4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder T4BasedOnT3Historized()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireHouseNumberWithOneSubaddressAndOneRetiredSubaddress()
        {
            Assert(UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T4BasedOnT3Historized())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV2, NewGebouweenheid1KeyV2, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireRetiredSubaddressT4BasedOnT3HistorizedTest()
        {
            Assert(UnretireRetiredSubaddressT4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder UnretireRetiredSubaddressT4BasedOnT3Historized()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(Fixture)
                .Given(T4BasedOnT3Historized())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3HistorizedTest()
        {
            Assert(UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3Historized()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(UnretireRetiredSubaddressT4BasedOnT3Historized())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV2, NewGebouweenheid1KeyV2, NewAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid4IdV2, NewGebouweenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), NewGebouweenheid4Id),

                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),

                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, NewAddress16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new BuildingUnitWasReaddressed(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.Address16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),

                    new BuildingUnitWasNotRealized(_.Gebouw1Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireForcedRetiredSubaddressTest()
        {
            Assert(UnretireForcedRetiredSubaddress());
        }

        public IEventCentricTestSpecificationBuilder UnretireForcedRetiredSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(Fixture)
                .Given(UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid4IdV2, NewGebouweenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), NewGebouweenheid4Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, NewGebouweenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3IdV2),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        // retire subaddress then change boxnumber => should not create new building unit
        [Fact]
        public void ImportSubaddressChangeWhenAlreadyRetiredByHouseNumber()
        {
            Assert(ImportSubaddressChangeWhenAlreadyRetiredBasedOnT3HistorizedHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressChangeWhenAlreadyRetiredBasedOnT3HistorizedHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3HistorizeDoIrrelevantChangeToHouseNumberTest()
        {
            Assert(BasedOnT3HistorizeDoIrrelevantChangeToHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3HistorizeDoIrrelevantChangeToHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3DoAddressIdHouseNumberChange()
        {
            var newCrabHouseNumberId = new CrabHouseNumberId(Fixture.Create<int>());
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))))
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(newCrabHouseNumberId); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T3BasedOnT2ReaddressSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, NewGebouweenheid4Id),

                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, NewAddress16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),

                    new BuildingUnitWasAdded(_.Gebouw1Id, NewGebouweenheid1IdV1, NewGebouweenheid1KeyV2, AddressId.CreateFor(newCrabHouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3DoAddressIdHouseNumberChangeTest()
        {
            Assert(BasedOnT3DoAddressIdHouseNumberChange());
        }
    }
}
