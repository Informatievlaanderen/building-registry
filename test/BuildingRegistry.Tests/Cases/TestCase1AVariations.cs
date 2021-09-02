namespace BuildingRegistry.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using NodaTime;
    using ValueObjects;
    using WhenImportingCrabSubaddress;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class TestCase1AVariations : TestCase1A
    {
        #region Snapshot variables

        private ImportSubaddressFromCrab? _importUnretireSubaddress2FromCrab;
        private ImportSubaddressFromCrab? _importRetireSubaddress2FromCrab;
        private ImportTerrainObjectHouseNumberFromCrab? _importRetireTerrainObjectHouseNumberFromCrab;

        #endregion

        private BuildingUnitId GebouwEenheid1Idv2 => BuildingUnitId.Create(_.GebouwEenheid1Key, 2);

        public TestCase1AVariations(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        public IEventCentricTestSpecificationBuilder BasedOnT2HistorizeHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T2())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT2HistorizedHouseNumberImportSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2HistorizeHouseNumber())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid4Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT2HistorizeHouseNumberTest()
        {
            Assert(BasedOnT2HistorizeHouseNumber());
        }

        [Fact]
        public void BasedOnT2HistorizeHouseNumberThenImportSubaddressShouldBeHistorized()
        {
            Assert(BasedOnT2HistorizedHouseNumberImportSubaddress());
        }


        public IEventCentricTestSpecificationBuilder BasedOnT3HistorizeHouseNumberSpecification()
        {
            _importRetireTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr16Id);

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(_importRetireTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    _importRetireTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3HistorizeHouseNumber()
        {
            Assert(BasedOnT3HistorizeHouseNumberSpecification());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressHistorizedByHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus1Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

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
        public void UnretireHouseNumberWithOneSubaddressTest()
        {
            Assert(UnretireHouseNumberWithOneSubaddress());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWithOneSubaddress()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT2HistorizeHouseNumber())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
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
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireHouseNumberWithOneSubaddressAndOneRetiredSubaddress()
        {
            Assert(UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired());
        }

        [Fact]
        public void RetireSubaddressAlreadyRetiredByHouseNumber()
        {
            Assert(T4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder T4BasedOnT3Historized()
        {
            _importRetireSubaddress2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddHours(1))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(_importRetireSubaddress2FromCrab)
                .Then(_.Gebouw1Id,
                    _importRetireSubaddress2FromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T4BasedOnT3Historized())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressChangeWhenAlreadyRetiredBasedOnT4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void ImportSubaddressChangeWhenAlreadyRetired()
        {
            Assert(ImportSubaddressChangeWhenAlreadyRetiredBasedOnT4());
        }

        public IEventCentricTestSpecificationBuilder UnretireRetiredSubaddressT4BasedOnT3Historized()
        {
            _importUnretireSubaddress2FromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddHours(2))))
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(Fixture)
                .Given(T4BasedOnT3Historized())
                .When(_importUnretireSubaddress2FromCrab)
                .Then(_.Gebouw1Id,
                    _importUnretireSubaddress2FromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireRetiredSubaddressT4BasedOnT3HistorizedTest()
        {
            Assert(UnretireRetiredSubaddressT4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3Historized()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            var buildingUnitWasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id);
            var buildingUnit2WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id);
            var commonBuildingUnitWasAdded = new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp));
            var buildingUnit3WasAdded = new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid4Id);

            return new AutoFixtureScenario(Fixture)
                .Given(UnretireRetiredSubaddressT4BasedOnT3Historized())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(
                    new Fact(_.Gebouw1Id, buildingUnitWasAdded),
                    new Fact(_.Gebouw1Id, buildingUnit2WasAdded),
                    new Fact(_.Gebouw1Id, commonBuildingUnitWasAdded),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2)),
                    new Fact(_.Gebouw1Id, buildingUnit3WasAdded),
                    new Fact(_.Gebouw1Id, new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1IdV2)),
                    new Fact(_.Gebouw1Id, new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3IdV2)),
                    new Fact(_.Gebouw1Id, importTerrainObjectHouseNumberFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                        {
                            { _.HuisNr16KoppelingId, _.HuisNr16Id }
                        })
                        .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId> { _.HuisNr16KoppelingId })
                        .WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>
                        {
                            { new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(_.HuisNr16KoppelingId, _.HuisNr16Id), new List<AddressSubaddressWasImportedFromCrab>
                                {
                                    _importSubaddressFromCrab.ToLegacyEvent(),
                                    _importSubaddress2FromCrab.ToLegacyEvent(),
                                    _importRetireSubaddress2FromCrab.ToLegacyEvent(),
                                    _importUnretireSubaddress2FromCrab.ToLegacyEvent(),
                                }
                            }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address16Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit2WasAdded)
                                    .WithRetiredByParent()
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address16Bus1Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Retired)
                                    .WithPreviousAddressId(_.Address16Id),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(_buildingUnit3WasAdded)
                                    .WithRetiredByParent()
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address16Bus2Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithPreviousAddressId(_.Address16Id)
                                    .WithAddressIds(new List<AddressId>()),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit2WasAdded),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(commonBuildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.Realized)
                                    .WithAddressIds(new List<AddressId>{_.Address16Id}),

                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnit3WasAdded)

                            }))
                        .Build(29, EventSerializerSettings)));
        }

        [Fact]
        public void UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3HistorizedTest()
        {
            Assert(UnretireHouseNumberBasedOnUnretireRetiredSubaddressT4BasedOnT3Historized());
        }

        public IEventCentricTestSpecificationBuilder UnretireForcedRetiredSubaddress()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null));

            return new AutoFixtureScenario(Fixture)
                .Given(UnretireHouseNumberWithOneSubaddressesAndOneSubaddressForceRetired())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp), _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3IdV2),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireForcedRetiredSubaddressTest()
        {
            Assert(UnretireForcedRetiredSubaddress());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddressChangeWhenAlreadyRetiredBasedOnT3HistorizedHouseNumber()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        // retire subaddress then change boxnumber => should not create new building unit
        [Fact]
        public void ImportSubaddressChangeWhenAlreadyRetiredByHouseNumber()
        {
            Assert(ImportSubaddressChangeWhenAlreadyRetiredBasedOnT3HistorizedHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder ImportRetiredSubaddressDeleteBasedOnT4()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus2Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(T4())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void ImportRetiredSubaddressDeleteDoesNotTryToCreateNewBuildingUnit()
        {
            Assert(ImportRetiredSubaddressDeleteBasedOnT4());
        }

        public IEventCentricTestSpecificationBuilder ImportSubaddress3()
        {
            var importSubaddressFromCrab = Fixture.Create<ImportSubaddressFromCrab>()
                .WithSubaddressId(_.SubaddressNr16Bus3Id)
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId);

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importSubaddressFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid6Key, _.Address16Bus3Id, new BuildingUnitVersion(importSubaddressFromCrab.Timestamp)),
                    importSubaddressFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3AddSubbaddress()
        {
            Assert(ImportSubaddress3());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3HistorizeDoIrrelevantChangeToHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3HistorizeHouseNumberSpecification())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3HistorizeDoIrrelevantChangeToHouseNumberTest()
        {
            Assert(BasedOnT3HistorizeDoIrrelevantChangeToHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3DoIrrelevantChangeToHouseNumber()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }


        [Fact]
        public void BasedOnT3DoIrrelevantChangeToHouseNumberTest()
        {
            Assert(BasedOnT3DoIrrelevantChangeToHouseNumber());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3DoAddressIdHouseNumberChange()
        {
            var newCrabHouseNumberId = new CrabHouseNumberId(Fixture.Create<int>());
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithTerrainObjectHouseNumberId(_.HuisNr16KoppelingId)
                .WithHouseNumberId(newCrabHouseNumberId); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(T3())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid2Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid3Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid4Id),

                    new BuildingUnitWasRemoved(_.Gebouw1Id, _.GebouwEenheid1Id),

                    new BuildingUnitWasAdded(_.Gebouw1Id, GebouwEenheid1Idv2, _.GebouwEenheid1Key, AddressId.CreateFor(newCrabHouseNumberId), new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3DoAddressIdHouseNumberChangeTest()
        {
            Assert(BasedOnT3DoAddressIdHouseNumberChange());
        }

        public IEventCentricTestSpecificationBuilder BasedOnT3And3SubaddressesRetireHouseNumberSpecification()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()))
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(ImportSubaddress3())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid2Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus1Id, _.GebouwEenheid2Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid4Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus2Id, _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealizedByParent(_.Gebouw1Id, _.GebouwEenheid6Id, _.GebouwEenheid1Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Bus3Id, _.GebouwEenheid6Id),
                    new BuildingUnitWasRetired(_.Gebouw1Id, _.GebouwEenheid3Id),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void BasedOnT3And3SubaddressesRetireHouseNumber()
        {
            Assert(BasedOnT3And3SubaddressesRetireHouseNumberSpecification());
        }

        public IEventCentricTestSpecificationBuilder UnretireHouseNumberWith3SubaddressesSpecification()
        {
            var importTerrainObjectHouseNumberFromCrab = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), null))
                .WithModification(CrabModification.Correction)
                .WithHouseNumberId(_.HuisNr16Id); //koppel huisnr 16

            return new AutoFixtureScenario(Fixture)
                .Given(BasedOnT3And3SubaddressesRetireHouseNumberSpecification())
                .When(importTerrainObjectHouseNumberFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid1IdV2, _.GebouwEenheid1Key, _.Address16Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid1Id),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid2IdV2, _.GebouwEenheid2Key, _.Address16Bus1Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid2Id),
                    new CommonBuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid3IdV2, _.GebouwEenheid3Key, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp)),
                    new BuildingUnitWasRealized(_.Gebouw1Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid4IdV2, _.GebouwEenheid4Key, _.Address16Bus2Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid4Id),
                    new BuildingUnitWasNotRealized(_.Gebouw1Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasDetached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid1IdV2),
                    new BuildingUnitAddressWasAttached(_.Gebouw1Id, _.Address16Id, _.GebouwEenheid3IdV2),
                    new BuildingUnitWasAdded(_.Gebouw1Id, _.GebouwEenheid6IdV2, _.GebouwEenheid6Key, _.Address16Bus3Id, new BuildingUnitVersion(importTerrainObjectHouseNumberFromCrab.Timestamp), _.GebouwEenheid6Id),
                    importTerrainObjectHouseNumberFromCrab.ToLegacyEvent());
        }

        [Fact]
        public void UnretireHouseNumberWith3Subaddresses()
        {
            Assert(UnretireHouseNumberWith3SubaddressesSpecification());
        }
    }
}
