namespace BuildingRegistry.Projections.Wfs.BuildingV2
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    [ConnectedProjectionName("WFS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WFS gebouwenregister voorziet.")]
    public class BuildingV2Projections : ConnectedProjection<WfsContext>
    {
        private static readonly string RealizedStatus = GebouwStatus.Gerealiseerd.ToString();
        private static readonly string PlannedStatus = GebouwStatus.Gepland.ToString();
        private static readonly string RetiredStatus = GebouwStatus.Gehistoreerd.ToString();
        private static readonly string NotRealizedStatus = GebouwStatus.NietGerealiseerd.ToString();
        private static readonly string UnderConstructionStatus = GebouwStatus.InAanbouw.ToString();
        public static readonly string MeasuredMethod = GeometrieMethode.IngemetenGRB.ToString();
        public static readonly string OutlinedMethod = GeometrieMethode.Ingeschetst.ToString();

        private readonly WKBReader _wkbReader;

        public BuildingV2Projections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = MapStatus(BuildingStatus.Parse(message.Message.BuildingStatus)),
                    IsRemoved = message.Message.IsRemoved,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV2, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = PlannedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV2, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = RealizedStatus,
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV2, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = UnderConstructionStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = NotRealizedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = PlannedStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item, message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                SetGeometry(
                    item,
                    message.Message.ExtendedWkbGeometryBuilding,
                    MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = RetiredStatus;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.IsRemoved = true;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Version = message.Message.Provenance.Timestamp;
            });

            // When<Envelope<BuildingMergerWasRealized>>(async (context, message, ct) =>
            // {
            //     var buildingV2 = new BuildingV2
            //     {
            //         PersistentLocalId = message.Message.BuildingPersistentLocalId,
            //         Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
            //         Status = RealizedStatus,
            //         IsRemoved = false,
            //         Version = message.Message.Provenance.Timestamp
            //     };
            //
            //     SetGeometry(
            //         buildingV2, message.Message.ExtendedWkbGeometry,
            //         MapGeometryMethod(BuildingGeometryMethod.MeasuredByGrb));
            //
            //     await context.BuildingsV2.AddAsync(buildingV2, ct);
            // });
            //
            // When<Envelope<BuildingWasMerged>>(async (context, message, ct) =>
            // {
            //     var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
            //     item.Status = RetiredStatus;
            //     item.Version = message.Message.Provenance.Timestamp;
            // });
            //
            // When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            // {
            //     var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
            //     item.Version = message.Message.Provenance.Timestamp;
            // });
            //
            // When<Envelope<BuildingUnitWasMoved>>(async (context, message, ct) =>
            // {
            //     var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
            //     item.Version = message.Message.Provenance.Timestamp;
            // });
        }

        private void SetGeometry(BuildingV2 building, string extendedWkbGeometry, string method)
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
    }
}
