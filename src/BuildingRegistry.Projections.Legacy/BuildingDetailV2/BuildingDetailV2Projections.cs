namespace BuildingRegistry.Projections.Legacy.BuildingDetailV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;

    [ConnectedProjectionName("API endpoint detail/lijst gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen detail & lijst voorziet.")]
    public class BuildingDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingDetailV2Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Parse(message.Message.GeometryMethod),
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Parse(message.Message.BuildingStatus),
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                UpdateHash(building, message);

                await context
                    .BuildingDetailsV2
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Outlined,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingStatus.Planned,
                    false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(building, message);

                await context
                    .BuildingDetailsV2
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Geometry = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Realized;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.NotRealized;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = true;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = true;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = false;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });
        }

        private static void UpdateHash<T>(BuildingDetailItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }
    }
}
