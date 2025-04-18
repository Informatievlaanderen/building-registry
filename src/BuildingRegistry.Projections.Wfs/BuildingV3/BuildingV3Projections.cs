namespace BuildingRegistry.Projections.Wfs.BuildingV3
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    /// <summary>
    /// Fixes the projection of building version concerning buildingunit removal and removal correction.
    /// </summary>
    [ConnectedProjectionName("WFS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WFS gebouwenregister voorziet.")]
    public class BuildingV3Projections : ConnectedProjection<WfsContext>
    {
        private static readonly string RealizedStatus = GebouwStatus.Gerealiseerd.ToString();
        private static readonly string PlannedStatus = GebouwStatus.Gepland.ToString();
        private static readonly string RetiredStatus = GebouwStatus.Gehistoreerd.ToString();
        private static readonly string NotRealizedStatus = GebouwStatus.NietGerealiseerd.ToString();
        private static readonly string UnderConstructionStatus = GebouwStatus.InAanbouw.ToString();
        public static readonly string MeasuredMethod = GeometrieMethode.IngemetenGRB.ToString();
        public static readonly string OutlinedMethod = GeometrieMethode.Ingeschetst.ToString();

        private readonly WKBReader _wkbReader;

        public BuildingV3Projections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingV3 = new BuildingV3
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = MapStatus(BuildingStatus.Parse(message.Message.BuildingStatus)),
                    IsRemoved = message.Message.IsRemoved,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV3, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingsV3.AddAsync(buildingV3, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingV3 = new BuildingV3
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = PlannedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV3, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));

                await context.BuildingsV3.AddAsync(buildingV3, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var buildingV3 = new BuildingV3
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = RealizedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV3, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));

                await context.BuildingsV3.AddAsync(buildingV3, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = NotRealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item,
                    message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RetiredStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = true;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV3.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
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

        private void SetGeometry(BuildingV3 building, string extendedWkbGeometry, string method)
        {
            var geometry = _wkbReader.Read(extendedWkbGeometry.ToByteArray()) as Polygon;
            geometry = geometry == null ? null : new GrbPolygon(geometry);

            building.GeometryMethod = method;
            building.Geometry = geometry;
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
