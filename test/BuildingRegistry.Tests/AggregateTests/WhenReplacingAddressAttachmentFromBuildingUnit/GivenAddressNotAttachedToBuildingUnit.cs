namespace BuildingRegistry.Tests.AggregateTests.WhenReplacingAddressAttachmentFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Building;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressReaddressed : BuildingRegistryTest
    {
        public GivenAddressReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void StateCheck_OnlyPreviousWasAttached()
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .WithAddress(otherAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                newAddressPersistentLocalId
            );
            @event.SetFixtureProvenance(Fixture);

            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated, @event });

            // Assert
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().HaveCount(2);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(newAddressPersistentLocalId);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.BuildingUnits.First().LastEventHash.Should().Be(@event.GetHash());
            sut.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
        }

        [Fact]
        public void StateCheck_BothPreviousAndNewWereAlreadyAttached()
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(newAddressPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .WithAddress(otherAddressPersistentLocalId)
                    .Build())
                .Build();

            var @event = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                newAddressPersistentLocalId
            );
            @event.SetFixtureProvenance(Fixture);

            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated, @event });

            // Assert
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().HaveCount(3);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Where(x => x == newAddressPersistentLocalId).Should().HaveCount(2);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().NotContain(previousAddressPersistentLocalId);
            sut.BuildingUnits.First().LastEventHash.Should().Be(@event.GetHash());
            sut.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
        }

        [Fact]
        public void StateCheck_BothPreviousAndNewWereAlreadyAttached_ReaddressTwice()
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var previousAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var newAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var otherAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithPersistentLocalId(buildingUnitPersistentLocalId)
                    .WithAddress(newAddressPersistentLocalId)
                    .WithAddress(previousAddressPersistentLocalId)
                    .WithAddress(otherAddressPersistentLocalId)
                    .Build())
                .Build();

            var firstEvent = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                previousAddressPersistentLocalId,
                newAddressPersistentLocalId
            );
            firstEvent.SetFixtureProvenance(Fixture);

            var secondEvent = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                newAddressPersistentLocalId,
                previousAddressPersistentLocalId
            );
            secondEvent.SetFixtureProvenance(Fixture);

            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated, firstEvent, secondEvent });

            // Assert
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().HaveCount(3);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Where(x => x == newAddressPersistentLocalId).Should().HaveCount(1);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Where(x => x == previousAddressPersistentLocalId).Should().HaveCount(1);
            sut.BuildingUnits.First().AddressPersistentLocalIds.Should().Contain(otherAddressPersistentLocalId);

            sut.BuildingUnits.First().LastEventHash.Should().Be(secondEvent.GetHash());
            sut.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
        }
    }
}
