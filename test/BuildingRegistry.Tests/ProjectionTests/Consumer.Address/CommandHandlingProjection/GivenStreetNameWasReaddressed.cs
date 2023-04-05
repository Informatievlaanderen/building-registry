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
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Xunit.Abstractions;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance;

    public sealed class GivenStreetNameWasReaddressed : BaseCommandHandlingProjectionTests
    {
        public GivenStreetNameWasReaddressed(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
        }

        [Fact]
        public async Task ThenDetachSourceAddressAndAttachToDestinationAddress()
        {
            var buildingPersistentLocalId = 11;
            var buildingUnitPersistentLocalId = 33;

            var readdressedHouseNumber =  new ReaddressedAddressData(
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

            var readdressedBoxNumber =  new ReaddressedAddressData(
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

            FakeBackOfficeContext.BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                readdressedHouseNumber.SourceAddressPersistentLocalId));
            FakeBackOfficeContext.BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                readdressedBoxNumber.SourceAddressPersistentLocalId));
            FakeBackOfficeContext.SaveChanges();

            var @event = new AddressHouseNumberWasReaddressed(
                streetNamePersistentLocalId: 0,
                addressPersistentLocalId: 1,
                readdressedHouseNumber : readdressedHouseNumber,
                readdressedBoxNumbers: new List<ReaddressedAddressData>(){ readdressedBoxNumber },
                rejectedBoxNumberAddressPersistentLocalIds: new List<int>(),
                retiredBoxNumberAddressPersistentLocalIds: new List<int>(),
                new Provenance(
                    Instant.FromDateTimeOffset(DateTimeOffset.Now).ToString(),
                    Application.ParcelRegistry.ToString(),
                    Modification.Update.ToString(),
                    Organisation.Aiv.ToString(),
                    "test"));

            Given(@event);

            await Then(async _ =>
            {
                MockCommandHandler.Verify(x => x.Handle(
                    It.Is<ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed>(x =>
                        x.SourceAddressPersistentLocalId == readdressedHouseNumber.SourceAddressPersistentLocalId &&
                        x.DestinationAddressPersistentLocalId == readdressedHouseNumber.DestinationAddressPersistentLocalId),
                    CancellationToken.None), Times.Exactly(1));


                MockCommandHandler.Verify(x => x.Handle(
                    It.Is<ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed>(x =>
                        x.SourceAddressPersistentLocalId == readdressedBoxNumber.SourceAddressPersistentLocalId &&
                        x.DestinationAddressPersistentLocalId == readdressedBoxNumber.DestinationAddressPersistentLocalId),
                    CancellationToken.None), Times.Exactly(1));

                await Task.CompletedTask;
            });

            var houseNumberRelation = await FakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                buildingUnitPersistentLocalId,
                readdressedHouseNumber.DestinationAddressPersistentLocalId);
            houseNumberRelation.Should().NotBeNull();

            var boxNumberRelation = await FakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                buildingUnitPersistentLocalId,
                readdressedBoxNumber.DestinationAddressPersistentLocalId);
            boxNumberRelation.Should().NotBeNull();
        }
    }
}
