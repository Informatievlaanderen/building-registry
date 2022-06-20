namespace BuildingRegistry.Tests.Legacy.Cases
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase1CVariations : TestCase1C
    {
        public TestCase1CVariations(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Gebouw1CrabTerrainObjectId = Fixture.Create<CrabTerrainObjectId>();
            HuisNr18KoppelingId = new CrabTerrainObjectHouseNumberId(Fixture.Create<int>());
            HuisNr18Id = new CrabHouseNumberId(Fixture.Create<int>());
            SubaddressNr18Bus1Id = new CrabSubaddressId(181);
            SubaddressNr18Bus2Id = new CrabSubaddressId(182);
        }

        public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }
        public CrabTerrainObjectHouseNumberId HuisNr18KoppelingId { get; }
        public CrabHouseNumberId HuisNr18Id { get; }
        public CrabSubaddressId SubaddressNr18Bus1Id { get; }
        public CrabSubaddressId SubaddressNr18Bus2Id { get; }

        public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);

        public BuildingUnitKey GebouwEenheid1Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId);

        public BuildingUnitKey GebouwEenheid2Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus1Id);

        public BuildingUnitKey GebouwEenheid3Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId); //gemeenschappelijk deel

        public BuildingUnitKey GebouwEenheid4Key =>
            BuildingUnitKey.Create(Gebouw1CrabTerrainObjectId, HuisNr18KoppelingId, SubaddressNr18Bus2Id);

        public BuildingUnitId GebouwEenheid1Id => BuildingUnitId.Create(GebouwEenheid1Key, 1);
        public BuildingUnitId GebouwEenheid2Id => BuildingUnitId.Create(GebouwEenheid2Key, 1);
        public BuildingUnitId GebouwEenheid3Id => BuildingUnitId.Create(GebouwEenheid3Key, 2);
        public BuildingUnitId GebouwEenheid4Id => BuildingUnitId.Create(GebouwEenheid4Key, 1);
        public AddressId Address18Id => AddressId.CreateFor(HuisNr18Id);
        public AddressId Address18Bus1Id => AddressId.CreateFor(SubaddressNr18Bus1Id);
        public AddressId Address18Bus2Id => AddressId.CreateFor(SubaddressNr18Bus2Id);

        // No need to read building because it will be deleted by houseNumber anyway.
        public IEventCentricTestSpecificationBuilder BasedOnT3DeleteHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void RemoveHouseNumberWhenRetiredWithActiveSubaddressUnits()
        {
            Assert(BasedOnT3DeleteHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2DeleteHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id)
                .WithModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void RemoveHouseNumberWhenNotRetiredWithActiveSubaddressUnit()
        {
            Assert(BasedOnT2DeleteHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3DeletedHouseNumberImportNewHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(HuisNr18KoppelingId)
                .WithHouseNumberId(HuisNr18Id);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3DeleteHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid1Id, GebouwEenheid1Key, Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void AddNewHouseNumberAfterDeletedT3()
        {
            Assert(BasedOnT3DeletedHouseNumberImportNewHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3DeletedHouseNumberImportedNewHouseNumberAddSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(SubaddressNr18Bus1Id)
                .WithHouseNumberId(HuisNr18Id)
                .WithTerrainObjectHouseNumberId(HuisNr18KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3DeletedHouseNumberImportNewHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid2Id, GebouwEenheid2Key, Address18Bus1Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid3Id, GebouwEenheid3Key, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void AddNewSubaddressToNewHouseNumber()
        {
            Assert(BasedOnT3DeletedHouseNumberImportedNewHouseNumberAddSubaddress());
        }
    }
}
