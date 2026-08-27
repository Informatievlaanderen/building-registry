namespace BuildingRegistry.Projections.Wfs.BuildingV4
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
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
    [ConnectedProjectionName("WFS gebouwen (v4, Lambert 2008)")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data in Lambert 2008 voor het WFS gebouwenregister voorziet.")]
    public class BuildingV4Projections : ConnectedProjection<WfsContext>
    {
        private static readonly string RealizedStatus = GebouwStatus.Gerealiseerd.ToString();
        private static readonly string PlannedStatus = GebouwStatus.Gepland.ToString();
        private static readonly string RetiredStatus = GebouwStatus.Gehistoreerd.ToString();
        private static readonly string NotRealizedStatus = GebouwStatus.NietGerealiseerd.ToString();
        private static readonly string UnderConstructionStatus = GebouwStatus.InAanbouw.ToString();
        public static readonly string MeasuredMethod = GeometrieMethode.IngemetenGRB.ToString();
        public static readonly string OutlinedMethod = GeometrieMethode.Ingeschetst.ToString();

        public BuildingV4Projections()
        {
            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingV4 = new BuildingV4
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = MapStatus(BuildingStatus.Parse(message.Message.BuildingStatus)),
                    IsRemoved = message.Message.IsRemoved,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV4, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingsV4.AddAsync(buildingV4, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingV4 = new BuildingV4
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = PlannedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV4, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));

                await context.BuildingsV4.AddAsync(buildingV4, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var buildingV4 = new BuildingV4
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = RealizedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV4, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));

                await context.BuildingsV4.AddAsync(buildingV4, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = NotRealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item,
                    message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RetiredStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV4.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = true;
                item.Version = message.Message.Provenance.Timestamp;
            });

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
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(DoNothing);
            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);
        }

        private static void SetGeometry(BuildingV4 building, string extendedWkbGeometry, string method)
        {
            building.GeometryMethod = method;
            building.Geometry = ParseGeometry(extendedWkbGeometry);
        }

        /// <summary>
        /// Version 4 stores Lambert 2008 (EPSG 3812) and nothing else, whichever reference system the
        /// event store persists, so the table, its spatial index and the views over it stay single-SRID.
        /// Once the event store holds Lambert 2008 this becomes a pass-through. See ADR 0005.
        /// </summary>
        private static Polygon? ParseGeometry(string extendedWkbGeometry)
        {
            var extendedWkb = extendedWkbGeometry.ToByteArray();

            if (WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb) is not Polygon geometry)
            {
                return null;
            }

            // A geometry already in Lambert 2008 is stored exactly as persisted; a transformed one is
            // stored at the precision the transform produces. A building outline is a polygon, and rounding
            // one moves every vertex and so its area, so it is not rounded. See ADR 0005.
            return
                geometry.IsLambert08()
                    ? new GrbPolygon(geometry)
                    : new GrbPolygon((Polygon)geometry.EnsureLambert08());
        }

        public static string MapGeometryMethod(BuildingGeometryMethod buildingGeometryMethod)
        {
            var dictionary = new Dictionary<BuildingGeometryMethod, string>
            {
                {BuildingGeometryMethod.Outlined, OutlinedMethod},
                {BuildingGeometryMethod.MeasuredByGrb, MeasuredMethod},
            };

            return dictionary[buildingGeometryMethod];
        }

        public static string MapStatus(BuildingStatus buildingStatus)
        {
            var dictionary = new Dictionary<BuildingStatus, string>
            {
                {BuildingStatus.Planned, PlannedStatus},
                {BuildingStatus.UnderConstruction, UnderConstructionStatus},
                {BuildingStatus.Realized, RealizedStatus},
                {BuildingStatus.NotRealized, NotRealizedStatus},
                {BuildingStatus.Retired, RetiredStatus},
            };

            return dictionary[buildingStatus];
        }

        private static Task DoNothing<T>(WfsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
