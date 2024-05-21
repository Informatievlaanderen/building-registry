namespace BuildingRegistry.Tests.AggregateTests.WhenReplacingAddressAttachmentFromBuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Building;
    using Building.Events;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressReaddressed : BuildingRegistryTest
    {
        public GivenAddressReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void StateCheck()
        {
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
            sut.LastEventHash.Should().NotBe(@event.GetHash());
        }
    }
}
