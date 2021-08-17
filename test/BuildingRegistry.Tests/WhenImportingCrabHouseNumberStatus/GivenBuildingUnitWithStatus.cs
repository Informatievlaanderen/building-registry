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
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitWithStatus : SnapshotBasedTest
    {
        public GivenBuildingUnitWithStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Theory]
        [InlineData(CrabAddressStatus.InUse)]
        [InlineData(CrabAddressStatus.OutOfUse)]
        [InlineData(CrabAddressStatus.Unofficial)]
        public void WithStatusPlannedWhenStatusMapsToRealized(CrabAddressStatus status)
        {
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
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
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
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
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
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
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
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
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
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
            var importStatus = Fixture.Create<ImportHouseNumberStatusFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            var buildingUnitPositionWasDerivedFromObject = Fixture.Create<BuildingUnitPositionWasDerivedFromObject>();
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    buildingUnitPositionWasDerivedFromObject,
                    Fixture.Create<BuildingUnitWasRealized>(),
                    Fixture.Create<BuildingUnitBecameComplete>())
                .When(importStatus)
                .Then(new Fact[]
                    {
                        new Fact(buildingId, new BuildingUnitStatusWasRemoved(buildingId, Fixture.Create<BuildingUnitId>())),
                        new Fact(buildingId, new BuildingUnitBecameIncomplete(buildingId, Fixture.Create<BuildingUnitId>())),
                        new Fact(buildingId, importStatus.ToLegacyEvent()),
                        new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithHouseNumberStatusEventsByHouseNumberId(new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>
                            {
                                { AddressId.CreateFor(importStatus.HouseNumberId), new List<AddressHouseNumberStatusWasImportedFromCrab>{ importStatus.ToLegacyEvent() } }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder
                                        .CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .BecameComplete(false)
                                        .WithHouseNumberStatusChronicle(new List<AddressHouseNumberStatusWasImportedFromCrab>
                                        {
                                            importStatus.ToLegacyEvent()
                                        })
                                        .WithPosition(new BuildingUnitPosition(new ExtendedWkbGeometry(buildingUnitPositionWasDerivedFromObject.ExtendedWkbGeometry), BuildingUnitPositionGeometryMethod.DerivedFromObject))
                                }))
                            .Build(7, EventSerializerSettings)
                        )
                    }));
        }
    }
}
