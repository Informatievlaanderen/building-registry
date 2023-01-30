namespace BuildingRegistry.Tests.Legacy.WhenReaddressingHouseNumber
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
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class CaseRetireAfterReaddressed : AutofacBasedTest
    {
        protected readonly IFixture Fixture;
        private LocalDate _readdressingBeginDate;

        private CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        private CrabTerrainObjectHouseNumberId OldHuisNr16KoppelingId { get; }
        private CrabHouseNumberId OldHuisNr16Id { get; }

        private CrabTerrainObjectHouseNumberId NewHuisNr16KoppelingId { get; }
        private CrabHouseNumberId NewHuisNr16Id { get; }
        public ReaddressingBeginDate ReaddressingBeginDate { get; }

        private BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
        private BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, OldHuisNr16KoppelingId);
        private BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        private AddressId OldAddress16Id => AddressId.CreateFor(OldHuisNr16Id);
        private AddressId NewAddress16Id => AddressId.CreateFor(NewHuisNr16Id);

        public CaseRetireAfterReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            OldHuisNr16KoppelingId = Fixture.Create<CrabTerrainObjectHouseNumberId>();
            NewHuisNr16KoppelingId = new CrabTerrainObjectHouseNumberId(Fixture.Create<int>());
            OldHuisNr16Id = Fixture.Create<CrabHouseNumberId>();
            NewHuisNr16Id = new CrabHouseNumberId(Fixture.Create<int>());
            _readdressingBeginDate = Fixture.Create<LocalDate>();
            ReaddressingBeginDate = new ReaddressingBeginDate(_readdressingBeginDate);
        }

        public IEventCentricTestSpecificationBuilder AddReaddressingOfHouseNumber()
        {
            var importReaddressingHouseNumber = Fixture.Create<ImportReaddressingHouseNumberFromCrab>()
                .WithOldTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithOldHouseNumberId(OldHuisNr16Id)
                .WithNewTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithNewHouseNumberId(NewHuisNr16Id)
                .WithBeginDate(ReaddressingBeginDate);

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(Gebouw1Id)
                .When(importReaddressingHouseNumber)
                .Then(Gebouw1Id,
                    importReaddressingHouseNumber.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddHouseNumberUnit()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(AddReaddressingOfHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    new BuildingUnitWasAdded(Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, OldAddress16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasReaddressed(Gebouw1Id, GebouwEenheid1Id, OldAddress16Id, NewAddress16Id, ReaddressingBeginDate),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(NewHuisNr16KoppelingId)
                .WithHouseNumberId(NewHuisNr16Id)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(1))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireOldHouseNr()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(OldHuisNr16KoppelingId)
                .WithHouseNumberId(OldHuisNr16Id)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(_readdressingBeginDate.ToDateTimeUnspecified().AddDays(2))));

            return new AutoFixtureScenario(Fixture)
                .Given(AddHouseNumberUnit())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void AddReaddressingHouseNumberTest()
        {
            Assert(AddReaddressingOfHouseNumber());
        }

        [Fact]
        public void AddHouseNumberUnitTest()
        {
            Assert(AddHouseNumberUnit());
        }

        [Fact]
        public void AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberIdTest()
        {
            Assert(AddReaddressedTerrainObjectHouseNumberWithNewHouseNumberId());
        }

        [Fact]
        public void RetireOldHouseNrTest()
        {
            Assert(RetireOldHouseNr());
        }
    }
}
