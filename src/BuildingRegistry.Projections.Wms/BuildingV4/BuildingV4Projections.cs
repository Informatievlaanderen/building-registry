namespace BuildingRegistry.Projections.Wms.BuildingV4
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;

    /// <summary>
    /// The Lambert 2008 (EPSG 3812) counterpart of <see cref="BuildingV3.BuildingV3Projections"/>, produced
    /// mechanically from it so the two stay identical apart from the reference system. See ADR 0005.
    /// </summary>
    [ConnectedProjectionName("WMS gebouwen (v4, Lambert 2008)")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data in Lambert 2008 voor het WMS gebouwregister voorziet.")]
    public class BuildingV4Projections : ConnectedProjection<WmsContext>
    {
        public const string MeasuredByGrbMethod = "IngemetenGRB";
        public const string OutlinedMethod = "Ingeschetst";

        /// <summary>
        /// Geometries are persisted at centimetre precision, which is what the Lambert transform is
        /// accurate to. See ADR 0005.
        /// </summary>
        private const int GeometryCoordinateDecimals = 2;

        public BuildingV4Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                {
                    return;
                }

                var buildingV2 = new BuildingV4
                {
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Version = message.Message.Provenance.Timestamp,
                    Status = BuildingStatus.Parse(message.Message.BuildingStatus),
                };

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, MapMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingsV4.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV4
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = BuildingStatus.Planned,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, OutlinedMethod);

                await context.BuildingsV4.AddAsync(buildingV2, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV4
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = BuildingStatus.Realized,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, MeasuredByGrbMethod);

                await context.BuildingsV4.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometryBuilding, OutlinedMethod);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometryBuilding, MeasuredByGrbMethod);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Realized;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.NotRealized;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometryBuilding, MeasuredByGrbMethod);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(item, message.Message.ExtendedWkbGeometryBuilding, MeasuredByGrbMethod);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Retired;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);

                context.BuildingsV4.Remove(item);
            });

            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);

            #region BuildingUnit

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
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
            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);

            #endregion
        }

        public static string MapMethod(BuildingGeometryMethod method)
        {
            var dictionary = new Dictionary<BuildingGeometryMethod, string>
            {
                { BuildingGeometryMethod.Outlined, OutlinedMethod },
                { BuildingGeometryMethod.MeasuredByGrb, MeasuredByGrbMethod }
            };

            return dictionary[method];
        }

        private static void SetGeometry(BuildingV4 building, string extendedWkbGeometry, string method)
        {
            building.GeometryMethod = method;
            building.Geometry = ParseGeometry(extendedWkbGeometry);
        }

        /// <summary>
        /// Version 4 stores Lambert 2008 (EPSG 3812) and nothing else, whichever reference system the
        /// event store persists. The bytes stored are plain WKB and carry no SRID of their own: the
        /// [wms].[BuildingsV4] computed column labels them 3812, and this is what keeps that label
        /// honest. Once the event store holds Lambert 2008 the transform becomes a no-op. See ADR 0005.
        /// </summary>
        private static byte[]? ParseGeometry(string extendedWkbGeometry)
        {
            var extendedWkb = extendedWkbGeometry.ToByteArray();

            if (WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb) is not Polygon geometry)
            {
                return null;
            }

            // Rounds only when it actually transforms, so a geometry already in Lambert 2008 is stored
            // exactly as persisted. The transform is accurate to the centimetre geometries are kept at.
            return
                geometry.IsLambert08()
                    ? geometry.AsBinary()
                    : ((Polygon)geometry.EnsureLambert08(GeometryCoordinateDecimals))
                        .AsBinary(); //asbinary is a must here since we are using a WKB and not EWKB
        }

        private static Task DoNothing<T>(WmsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
