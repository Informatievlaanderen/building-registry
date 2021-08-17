namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberStatus
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitIsNotRealized : SnapshotBasedTest
    {
        public GivenBuildingUnitIsNotRealized(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasNotRealized>())
                .When(importStatus)
                .Then(buildingId,
                   importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ByParent_ThenNothingHappens(CrabAddressStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasNotRealizedByParent>())
                .When(importStatus)
                .Then(new Fact[]
                {
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                        {
                            { AddressId.CreateFor(importStatus.HouseNumberId), new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() } }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder
                                    .CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithStatus(BuildingUnitStatus.NotRealized)
                                    .WithRetiredByParent()
                                    .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() })
                            }))
                        .Build(3, EventSerializerSettings))
                }));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Reserved)]
        [InlineData(CrabAddressStatus.Proposed)]
        public void ButRetiredByBuilding_ThenBecomesNotRealized(CrabAddressStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAddedToRetiredBuilding = Fixture.Create<BuildingUnitWasAddedToRetiredBuilding>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAddedToRetiredBuilding)
                .When(importStatus)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingUnitWasNotRealized(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                        {
                            { AddressId.CreateFor(importStatus.HouseNumberId), new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() } }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder
                                    .CreateDefaultSnapshotFor(buildingUnitWasAddedToRetiredBuilding, BuildingUnitStatus.NotRealized)
                                    .WithRetiredByBuilding(false)
                                    .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() })
                            }))
                        .Build(3, EventSerializerSettings))
                }));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ThenBecomesRetired(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasNotRealized>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ByParent_ThenBecomesRetired(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasNotRealizedByParent>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRetired(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void ButRetiredByBuilding_ThenNothingHappens(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAddedToRetiredBuilding>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }
    }
}
