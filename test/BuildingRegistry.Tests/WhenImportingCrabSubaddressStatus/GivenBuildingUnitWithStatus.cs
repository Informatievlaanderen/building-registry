namespace BuildingRegistry.Tests.WhenImportingCrabSubaddressStatus
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
    using WhenImportingCrabSubaddress;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitWithStatus : SnapshotBasedTest
    {
        public GivenBuildingUnitWithStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WithStatusPlannedWhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasPlanned>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasRealized(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WithStatusRealizedWhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasRealized>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Proposed)]
        [InlineData(CrabAddressStatus.Reserved)]
        public void WithStatusRealizedWhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasRealized>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasPlanned(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressStatus.Proposed)]
        [InlineData(CrabAddressStatus.Reserved)]
        public void WithStatusPlannedWhenStatusMapsToPlanned(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(status);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasPlanned>())
                .When(importStatus)
                .Then(buildingId,
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusProposedWhenStatusWasRemoved()
        {
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithStatus(CrabAddressStatus.Proposed);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingUnitWasPlanned>(),
                    Fixture.Create<BuildingUnitStatusWasRemoved>())
                .When(importStatus)
                .Then(buildingId,
                    new BuildingUnitWasPlanned(buildingId, Fixture.Create<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithDelete()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importStatus = Fixture.Create<ImportSubaddressStatusFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingUnitWasRealized>())
                .When(importStatus)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingUnitStatusWasRemoved(buildingId, Fixture.Create<BuildingUnitId>())),
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                        .WithSubaddressStatusEventsBySubaddressId(new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>
                        {
                            { importStatus.SubaddressId, new List<AddressSubaddressStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() } }
                        })
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithSubaddressStatusChronicle(new List<AddressSubaddressStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent()})
                            }))
                        .Build(4, EventSerializerSettings))
                }));
        }
    }
}
