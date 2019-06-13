namespace BuildingRegistry.Tests.Cases
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase2Variations : TestCase2
    {
        private BuildingUnitId GebouwEenheid1Idv2 => BuildingUnitId.Create(_.GebouwEenheid1Key, 2);
        private BuildingUnitId GebouwEenheid2Idv2 => BuildingUnitId.Create(_.GebouwEenheid2Key, 2);
        private BuildingUnitId GebouwEenheid3Idv2 => BuildingUnitId.Create(_.GebouwEenheid3Key, 2);

        public TestCase2Variations(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberId()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(_.HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(T1())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid2Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3Id, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdTest()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberId());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdT3()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);
            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberId())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key,
                        AddressId.CreateFor(_.SubaddressNr18Bus1Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdT4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr18Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId);
            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberIdT3())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid5Id, _.GebouwEenheid5Key, AddressId.CreateFor(_.SubaddressNr18Bus2Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid2Id),
                    //new AddressWasDetached(_.Gebouw1Id,_.Address18Id,  _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdT5()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberIdT4())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, AddressId.CreateFor(_.SubaddressNr16Bus1Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdT6()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);
            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberIdT5())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid7Id, _.GebouwEenheid7Key, AddressId.CreateFor(_.SubaddressNr16Bus2Id), new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdTestT3()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdT3());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdTestT4()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdT4());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdTestT5()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdT5());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdTestT6()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdT6());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdChangeAddress()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(_.HuisNr18Id);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberIdT6())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid6Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid7Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),

                    new BuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid1Idv2, _.GebouwEenheid1Key, _.Address18Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdChangeAddressTest()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdChangeAddress());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT1AddT2WithSameHouseNumberIdChangeAddressForOtherHouseNumber()
        {
            var crabHouseNumberId = new CrabHouseNumberId(Fixture.Create<int>());
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr18KoppelingId)
                .WithHouseNumberId(crabHouseNumberId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT1AddT2WithSameHouseNumberIdChangeAddress())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid5Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),

                    new BuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid2Idv2, _.GebouwEenheid2Key, AddressId.CreateFor(crabHouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),

                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid3Idv2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, GebouwEenheid3Idv2),

                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT1AddT2WithSameHouseNumberIdChangeAddressForOtherHouseNumberTest()
        {
            Assert(BasedOnT1AddT2WithSameHouseNumberIdChangeAddressForOtherHouseNumber());
        }
    }
}
