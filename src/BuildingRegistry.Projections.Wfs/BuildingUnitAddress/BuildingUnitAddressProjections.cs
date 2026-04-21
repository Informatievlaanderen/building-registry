namespace BuildingRegistry.Projections.Wfs.BuildingUnitAddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using Microsoft.EntityFrameworkCore;

    public sealed class BuildingUnitAddressProjections : ConnectedProjection<WfsContext>
    {
        public BuildingUnitAddressProjections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    foreach (var buildingUnitAddressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                    {
                        var buildingUnitV2 = new BuildingUnitAddress(buildingUnit.BuildingUnitPersistentLocalId, buildingUnitAddressPersistentLocalId);

                        await context.BuildingUnitAddresses.AddAsync(buildingUnitV2, ct);
                    }
                }
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                context.BuildingUnitAddresses.Add(new BuildingUnitAddress(message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId));
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var buAddress =
                    await context.BuildingUnitAddresses.FindAsync(
                        [message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId], ct);
                context.BuildingUnitAddresses.Remove(buAddress!);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var buAddress =
                    await context.BuildingUnitAddresses.FindAsync(
                        [message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId], ct);
                context.BuildingUnitAddresses.Remove(buAddress!);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var buAddress =
                    await context.BuildingUnitAddresses.FindAsync(
                        [message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId], ct);
                context.BuildingUnitAddresses.Remove(buAddress!);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var buAddress =
                    await context.BuildingUnitAddresses.FindAsync(
                        [message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId], ct);
                context.BuildingUnitAddresses.Remove(buAddress!);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var previousAddress = await context.BuildingUnitAddresses.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.PreviousAddressPersistentLocalId], ct);

                if (previousAddress is not null && previousAddress.Count == 1)
                {
                    context.BuildingUnitAddresses.Remove(previousAddress);
                }
                else if (previousAddress is not null)
                {
                    previousAddress.Count -= 1;
                }

                var newAddress = await context.BuildingUnitAddresses.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId], ct);

                if (newAddress is null)
                {
                    context.BuildingUnitAddresses.Add(new BuildingUnitAddress(message.Message.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId));
                }
                else
                {
                    var entry = context.Entry(newAddress);
                    if(entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                        newAddress.Count = 1;
                    }
                    else
                    {
                        newAddress.Count += 1;
                    }
                }
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitAddressesWereReaddressed in message.Message.BuildingUnitsReaddresses)
                {
                    foreach (var detachedAddressPersistentLocalId in buildingUnitAddressesWereReaddressed.DetachedAddressPersistentLocalIds)
                    {
                        var address = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnitAddressesWereReaddressed.BuildingUnitPersistentLocalId, detachedAddressPersistentLocalId], ct);

                        if (address is not null)
                        {
                            context.BuildingUnitAddresses.Remove(address);
                        }
                    }

                    foreach (var attachedAddressPersistentLocalId in buildingUnitAddressesWereReaddressed.AttachedAddressPersistentLocalIds)
                    {
                        var address = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnitAddressesWereReaddressed.BuildingUnitPersistentLocalId, attachedAddressPersistentLocalId], ct);
                        if (address is null)
                        {
                            context.BuildingUnitAddresses.Add(new BuildingUnitAddress(buildingUnitAddressesWereReaddressed.BuildingUnitPersistentLocalId, attachedAddressPersistentLocalId));
                        }
                        else
                        {
                            var entry = context.Entry(address);
                            if (entry.State == EntityState.Deleted)
                            {
                                entry.State = EntityState.Modified;
                            }
                        }
                    }
                }
            });
            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var previousAddress = await context.BuildingUnitAddresses.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.PreviousAddressPersistentLocalId], ct);

                if (previousAddress is not null)
                {
                    context.BuildingUnitAddresses.Remove(previousAddress);
                }

                var newAddress = await context.BuildingUnitAddresses.FindAsync(
                    [message.Message.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId], ct);

                if (newAddress is null)
                {
                    context.BuildingUnitAddresses.Add(new BuildingUnitAddress(message.Message.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId));
                }
                else
                {
                    var entry = context.Entry(newAddress);
                    if (entry.State == EntityState.Deleted)
                    {
                        entry.State = EntityState.Modified;
                    }
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(DoNothing);
            When<Envelope<BuildingWasMeasured>>(DoNothing);
            When<Envelope<BuildingMeasurementWasCorrected>>(DoNothing);
            When<Envelope<BuildingMeasurementWasChanged>>(DoNothing);
            When<Envelope<BuildingWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(DoNothing);

            When<Envelope<BuildingUnitWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredV2>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);
            When<Envelope<BuildingUnitRemovalWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasRegularized>>(DoNothing);
            When<Envelope<BuildingUnitRegularizationWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasDeregulated>>(DoNothing);
            When<Envelope<BuildingUnitDeregulationWasCorrected>>(DoNothing);
            When<Envelope<CommonBuildingUnitWasAddedV2>>(DoNothing);
            When<Envelope<BuildingUnitPositionWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(DoNothing);
            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
        }

        private static Task DoNothing<T>(WfsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
