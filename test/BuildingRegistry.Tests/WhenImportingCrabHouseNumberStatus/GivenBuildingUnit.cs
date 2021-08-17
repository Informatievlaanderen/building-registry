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

    public class GivenBuildingUnit : SnapshotBasedTest
    {
        public GivenBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            Fixture.Customize(new WithNoDeleteModification());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRealized(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Proposed)]
        [InlineData(CrabAddressStatus.Reserved)]
        public void WhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded)
                .When(importStatus)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingUnitWasPlanned(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                            {
                                { AddressId.CreateFor(importStatus.HouseNumberId), new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() } }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(
                                    new List<BuildingUnitSnapshot>{
                                        BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithStatus(BuildingUnitStatus.Planned)
                                        .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>
                                        {
                                            importStatus.ToLegacyEvent()
                                        })
                                    }))
                            .Build(3, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithEmptyStatusWhenModificationRemoved()
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusProposedWhenStatusIsRealizedAndNewerLifetime()
        {
            var importedStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Proposed)
                .WithLifetime(new CrabLifetime(importedStatus.Lifetime.BeginDateTime.Value.PlusDays(1), importedStatus.Lifetime.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasRealized>(),
                    importedStatus.ToLegacyEvent())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasPlanned(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusProposedWhenStatusIsRealizedAndOlderLifetime()
        {
            var importedStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.InUse);

            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Proposed)
                .WithLifetime(new CrabLifetime(importedStatus.Lifetime.BeginDateTime.Value.PlusDays(-1), importedStatus.Lifetime.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasRealized>(),
                    importedStatus.ToLegacyEvent())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        // rest of tests: see address registry for status logic => only test if implemented here
    }
}
