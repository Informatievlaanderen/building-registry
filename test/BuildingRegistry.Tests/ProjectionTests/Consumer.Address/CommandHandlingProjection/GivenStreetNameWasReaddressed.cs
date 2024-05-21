//TODO-jonas implement
namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Address.CommandHandlingProjection
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands;
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
            var buildingPersistentLocalId = 11;
            var buildingUnitPersistentLocalId = 33;

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
            _fakeBackOfficeContext.SaveChanges();

            var @event = new StreetNameWasReaddressed(
                streetNamePersistentLocalId: 0,
                new[]
                {
                    new AddressHouseNumberReaddressedData(1, readdressedHouseNumber, new List<ReaddressedAddressData> { readdressedBoxNumber })
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
                _mockCommandHandler.Verify(x => x.Handle(
                    It.Is<ReaddressAddresses>(readdressAddresses =>
                        readdressAddresses.BuildingPersistentLocalId == buildingPersistentLocalId
                        && readdressAddresses.Readdresses.Count == 2
                    ),
                    CancellationToken.None), Times.Exactly(1));

                await Task.CompletedTask;
            });

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
        }
    }
}
