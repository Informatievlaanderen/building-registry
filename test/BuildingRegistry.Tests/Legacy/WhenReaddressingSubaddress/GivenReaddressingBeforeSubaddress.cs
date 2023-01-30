namespace BuildingRegistry.Tests.Legacy.WhenReaddressingSubaddress
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using NodaTime;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenReaddressingBeforeSubaddress : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private LocalDate _readdressingBeginDate;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId HuisNr16KoppelingId { get; }
        private CrabHouseNumberId HuisNr16Id { get; }
        public CrabSubaddressId OldSubaddressNr16Bus1Id { get; }
        public CrabSubaddressId NewSubaddressNr16Bus1Id { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId);
        public BuildingUnitKey OldGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, OldSubaddressNr16Bus1Id);
        public BuildingUnitKey NewGebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr16KoppelingId, NewSubaddressNr16Bus1Id);
        public BuildingUnitKey GebouwEenheid3Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(OldGebouwEenheid2Key, 1);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 1);

        private AddressId Address16Id => AddressId.CreateFor(HuisNr16Id);

        public AddressId OldAddress16Bus1Id => AddressId.CreateFor(OldSubaddressNr16Bus1Id);
        public AddressId NewAddress16Bus1Id => AddressId.CreateFor(NewSubaddressNr16Bus1Id);

        public GivenReaddressingBeforeSubaddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            HuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            OldSubaddressNr16Bus1Id = new CrabSubaddressId(161);
            NewSubaddressNr16Bus1Id = new CrabSubaddressId(163);
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfSubaddress()
        {
            var importReaddressingSubaddress = Fixture.Create<ImportReaddressingSubaddressFromCrab>()
                .WithOldTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithOldSubaddressId(OldSubaddressNr16Bus1Id)
                .WithNewTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithNewSubaddressId(NewSubaddressNr16Bus1Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importReaddressingSubaddress)
                .Then(Gebouw1Id,
                    importReaddressingSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithHouseNumberId(HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfSubaddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddSubaddress()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(OldSubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(
                    Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(-1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid2Id, OldGebouwEenheid2Key, OldAddress16Bus1Id, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid2Id, OldAddress16Bus1Id, NewAddress16Bus1Id, ReaddressingBeginDate),
                    new CommonBuildingUnitWasAdded(Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(importSubaddress.Timestamp)),
                    new BuildingUnitWasRealized(Gebouw1Id, GebouwEenheid3Id),
                    importSubaddress.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedSubaddressWithNewSubaddressId()
        {
            var importSubaddress = Fixture.Create<ImportSubaddressFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr16KoppelingId)
                .WithSubaddressId(NewSubaddressNr16Bus1Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddSubaddress())
                .When(importSubaddress)
                .Then(Gebouw1Id,
                    importSubaddress.ToLegacyEvent());
        }

        [Fact]
        public void AddReaddressingSubaddressTest()
        {
            Assert(AddReaddressingOfSubaddress());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddSubaddressUnitTest()
        {
            Assert(AddSubaddress());
        }

        [Fact]
        public void AddReaddressedSubaddressWithNewSubaddressIdTest()
        {
            Assert(AddReaddressedSubaddressWithNewSubaddressId());
        }
    }
}
