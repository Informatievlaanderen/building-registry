namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address.CommandHandlingProjection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using Xunit;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public partial class CommandHandlingKafkaProjectionTests
    {
        [Fact]
        public async Task ThenDetachSourceAddressAndAttachToDestinationAddress()
        {
            Fixture.Customizations.Add(new WithUniqueInteger());

            var buildingPersistentLocalId = Fixture.Create<int>();
            var buildingUnitPersistentLocalId = Fixture.Create<int>();

            var readdressedHouseNumber = new ReaddressedAddressData(
                sourceAddressPersistentLocalId: 1,
                destinationAddressPersistentLocalId: 2,
                false,
                "proposed",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                true);

            var readdressedBoxNumber = new ReaddressedAddressData(
                sourceAddressPersistentLocalId: 3,
                destinationAddressPersistentLocalId: 4,
                false,
                "proposed",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                true);

            _fakeBackOfficeContext.BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                readdressedHouseNumber.SourceAddressPersistentLocalId));
            _fakeBackOfficeContext.BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                readdressedBoxNumber.SourceAddressPersistentLocalId));
            await _fakeBackOfficeContext.SaveChangesAsync();

            SetupBuildingAggregate(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new Dictionary<BuildingUnitPersistentLocalId, IEnumerable<AddressPersistentLocalId>>
                {
                    {
                        new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                        [
                            new AddressPersistentLocalId(readdressedHouseNumber.DestinationAddressPersistentLocalId),
                            new AddressPersistentLocalId(readdressedBoxNumber.DestinationAddressPersistentLocalId)
                        ]
                    }
                });

            var @event = new StreetNameWasReaddressed(
                streetNamePersistentLocalId: Fixture.Create<int>(),
                new[]
                {
                    new AddressHouseNumberReaddressedData(
                        readdressedHouseNumber.DestinationAddressPersistentLocalId,
                        readdressedHouseNumber,
                        new List<ReaddressedAddressData> { readdressedBoxNumber })
                },
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);

            await Then(async _ =>
            {
                _mockCommandHandler.Verify(
                    x =>
                        x.HandleIdempotent(
                            It.Is<ReaddressAddresses>(readdressAddresses =>
                                readdressAddresses.BuildingPersistentLocalId == buildingPersistentLocalId
                                && readdressAddresses.Readdresses.Count == 1
                                && readdressAddresses.Readdresses[new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)].Count == 2
                            ),
                            CancellationToken.None),
                    Times.Exactly(1));

                _mockCommandHandler.Invocations.Count.Should().Be(1);

                var sourceHouseNumberRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                    buildingUnitPersistentLocalId,
                    readdressedHouseNumber.SourceAddressPersistentLocalId);
                sourceHouseNumberRelation.Should().BeNull();

                var sourceBoxNumberRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                    buildingUnitPersistentLocalId,
                    readdressedBoxNumber.SourceAddressPersistentLocalId);
                sourceBoxNumberRelation.Should().BeNull();

                var destinationHouseNumberRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                    buildingUnitPersistentLocalId,
                    readdressedHouseNumber.DestinationAddressPersistentLocalId);
                destinationHouseNumberRelation.Should().NotBeNull();

                var destinationBoxNumberRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                    buildingUnitPersistentLocalId,
                    readdressedBoxNumber.DestinationAddressPersistentLocalId);
                destinationBoxNumberRelation.Should().NotBeNull();
            });
        }

        private void SetupBuildingAggregate(
            BuildingPersistentLocalId buildingPersistentLocalId,
            IDictionary<BuildingUnitPersistentLocalId, IEnumerable<AddressPersistentLocalId>> buildingUnitsWithAddresses)
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var events = buildingUnitsWithAddresses
                .SelectMany(x =>
                {
                    var buildingUnitWasPlannedV2 = new BuildingUnitWasPlannedV2(
                        buildingPersistentLocalId,
                        x.Key,
                        Fixture.Create<BuildingUnitPositionGeometryMethod>(),
                        Fixture.Create<ExtendedWkbGeometry>(),
                        Fixture.Create<BuildingUnitFunction>(),
                        Fixture.Create<bool>());
                    buildingUnitWasPlannedV2.SetFixtureProvenance(Fixture);

                    var addressEvents = x.Value.Select(y =>
                    {
                        var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                            buildingPersistentLocalId,
                            x.Key,
                            y);
                        buildingUnitAddressWasAttachedV2.SetFixtureProvenance(Fixture);
                        return buildingUnitAddressWasAttachedV2;
                    });

                    return new object[] { buildingUnitWasPlannedV2 }.Concat(addressEvents);
                })
                .ToList();

            building.Initialize(events);

            _buildings
                .Setup(x => x.GetAsync(new BuildingStreamId(buildingPersistentLocalId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(building);
        }
    }
}
