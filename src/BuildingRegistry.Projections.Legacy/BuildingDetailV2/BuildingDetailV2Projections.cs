namespace BuildingRegistry.Projections.Legacy.BuildingDetailV2
{
    using System;
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

    [ConnectedProjectionName("API endpoint detail/lijst gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het gebouwen detail & lijst voorziet.")]
    public class BuildingDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingDetailV2Projections()
        {
            var wkbReader = WKBReaderFactory.Create();

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Parse(message.Message.GeometryMethod),
                    geometryAsBinary,
                    fixedGeometry,
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
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.Outlined,
                    geometryAsBinary,
                    fixedGeometry,
                    BuildingStatus.Planned,
                    false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(building, message);

                await context
                    .BuildingDetailsV2
                    .AddAsync(building, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var building = new BuildingDetailItemV2(
                    message.Message.BuildingPersistentLocalId,
                    BuildingGeometryMethod.MeasuredByGrb,
                    geometryAsBinary,
                    fixedGeometry,
                    BuildingStatus.Realized,
                    false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(building, message);

                await context
                    .BuildingDetailsV2
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Geometry = geometryAsBinary;
                item.SysGeometry = fixedGeometry;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Geometry = geometryAsBinary;
                item.SysGeometry = fixedGeometry;
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

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Geometry = geometryAsBinary;
                item.SysGeometry = fixedGeometry;
                item.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);
                var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(sysGeometry);
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Geometry = geometryAsBinary;
                item.SysGeometry = fixedGeometry;
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Retired;
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

            #region BuildingUnit
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
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingDetailsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
                UpdateHash(item, message);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitWasRegularized>>(DoNothing);
            When<Envelope<BuildingUnitRegularizationWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasDeregulated>>(DoNothing);
            When<Envelope<BuildingUnitDeregulationWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitPositionWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(DoNothing);
            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasAttachedV2>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedV2>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);
            #endregion
        }

        private static void UpdateHash<T>(BuildingDetailItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }

        private static Task DoNothing<T>(LegacyContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
