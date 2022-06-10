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
    using NodaTime;

    [ConnectedProjectionName("WFS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WFS gebouwenregister voorziet.")]
    public class BuildingV2Projections : ConnectedProjection<WfsContext>
    {
        private static readonly string RealizedStatus = GebouwStatus.Gerealiseerd.ToString();
        private static readonly string PlannedStatus = GebouwStatus.Gepland.ToString();
        private static readonly string RetiredStatus = GebouwStatus.Gehistoreerd.ToString();
        private static readonly string NotRealizedStatus = GebouwStatus.NietGerealiseerd.ToString();
        private static readonly string UnderConstructionStatus = GebouwStatus.InAanbouw.ToString();
        private static readonly string MeasuredMethod = GeometrieMethode.IngemetenGRB.ToString();
        private static readonly string OutlinedMethod = GeometrieMethode.Ingeschetst.ToString();

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
                    Status = MapStatus(BuildingStatus.Planned),
                    IsRemoved = false,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(
                    buildingV2, message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingGeometryMethod.Outlined));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });
        }

        private static void SetVersion(BuildingV2 building, Instant provenanceTimestamp)
        {
            building.Version = provenanceTimestamp;
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
