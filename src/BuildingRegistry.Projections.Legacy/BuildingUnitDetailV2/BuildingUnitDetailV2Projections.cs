namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;

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
                        .Select(groupedAddressId => new BuildingUnitDetailAddressItemV2
                        {
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = groupedAddressId.Key,
                            Count = groupedAddressId.Count()
                        })
                        .ToList();

                    var buildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                        buildingUnit.BuildingUnitPersistentLocalId,
                        message.Message.BuildingPersistentLocalId,
                        buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        BuildingUnitFunction.Parse(buildingUnit.Function),
                        BuildingUnitStatus.Parse(buildingUnit.Status),
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
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(buildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2.AddAsync(
                    buildingUnitDetailItemV2
                    , ct);
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

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
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
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(commonBuildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2.AddAsync(
                    commonBuildingUnitDetailItemV2
                    , ct);
            });
        }

        private async Task Update(LegacyContext context, int buildingUnitPersistentLocalId, Action<BuildingUnitDetailItemV2> updateAction, CancellationToken ct)
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
