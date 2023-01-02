namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [ConnectedProjectionName("API endpoint detail/lijst gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het gebouweenheden detail & lijst voorziet.")]
    public class BuildingUnitDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingUnitDetailV2Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                BuildingStatus? retiredStatus = null;
                var buildingStatus = BuildingStatus.Parse(message.Message.BuildingStatus);
                if (buildingStatus == BuildingStatus.Retired || buildingStatus == BuildingStatus.NotRealized)
                {
                    retiredStatus = buildingStatus;
                }

                var building = new BuildingUnitBuildingItemV2(
                    message.Message.BuildingPersistentLocalId,
                    message.Message.IsRemoved,
                    retiredStatus);

                await context.BuildingUnitBuildingsV2.AddAsync(building, ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var addressesIdsGrouped = buildingUnit.AddressPersistentLocalIds.GroupBy(x => x);
                    var addresses = addressesIdsGrouped
                        .Select(groupedAddressId => new BuildingUnitDetailAddressItemV2(buildingUnit.BuildingUnitPersistentLocalId, groupedAddressId.Key))
                        .ToList();

                    var buildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                        buildingUnit.BuildingUnitPersistentLocalId,
                        message.Message.BuildingPersistentLocalId,
                        buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        BuildingUnitFunction.Parse(buildingUnit.Function),
                        BuildingUnitStatus.Parse(buildingUnit.Status),
                        false,
                        new Collection<BuildingUnitDetailAddressItemV2>(addresses),
                        buildingUnit.IsRemoved,
                        message.Message.Provenance.Timestamp);

                    UpdateHash(buildingUnitDetailItemV2, message);

                    await context.BuildingUnitDetailsV2.AddAsync(
                        buildingUnitDetailItemV2
                        , ct);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var building = new BuildingUnitBuildingItemV2(
                    message.Message.BuildingPersistentLocalId,
                    false,
                    null);

                await context.BuildingUnitBuildingsV2.AddAsync(building, ct);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Parse(message.Message.Function),
                    BuildingUnitStatus.Planned,
                    message.Message.HasDeviation,
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(buildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2.AddAsync(buildingUnitDetailItemV2, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await Update(context, buildingUnitPersistentLocalId, item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                        item.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Retired;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.IsRemoved = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.IsRemoved = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    var addresses = message.Message.AddressPersistentLocalIds
                        .Select(addressId => new BuildingUnitDetailAddressItemV2(message.Message.BuildingUnitPersistentLocalId, addressId))
                        .ToList();

                    item.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                    item.HasDeviation = message.Message.HasDeviation;
                    item.Function = BuildingUnitFunction.Parse(message.Message.Function);
                    item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                    item.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                    item.IsRemoved = false;
                    item.Addresses = new Collection<BuildingUnitDetailAddressItemV2>(addresses);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = false;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var commonBuildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Common,
                    BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                    message.Message.HasDeviation,
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(commonBuildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2.AddAsync(
                    commonBuildingUnitDetailItemV2
                    , ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                    item.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    item.Addresses.Add(new BuildingUnitDetailAddressItemV2(message.Message.BuildingUnitPersistentLocalId, message.Message.AddressPersistentLocalId));
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });
        }

        private static async Task Update(LegacyContext context, int buildingUnitPersistentLocalId, Action<BuildingUnitDetailItemV2> updateAction, CancellationToken ct)
        {
            var item = await context
                .BuildingUnitDetailsV2
                .FindAsync(buildingUnitPersistentLocalId, cancellationToken: ct);
            updateAction(item);
        }

        private static void UpdateHash<T>(BuildingUnitDetailItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }
    }
}
