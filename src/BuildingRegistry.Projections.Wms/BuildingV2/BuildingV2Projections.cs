namespace BuildingRegistry.Projections.Wms.BuildingV2
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("WMS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WMS gebouwregister voorziet.")]
    public class BuildingV2Projections : ConnectedProjection<WmsContext>
    {
        private readonly WKBReader _wkbReader;

        public BuildingV2Projections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                if (message.Message.IsRemoved)
                {
                    return;
                }

                var buildingV2 = new BuildingV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Version = message.Message.Provenance.Timestamp,
                    Status = BuildingStatus.Parse(message.Message.BuildingStatus),
                };

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, BuildingGeometryMethod.Parse(message.Message.GeometryMethod));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });
        }

        private static void SetVersion(BuildingV2 building, Instant provenanceTimestamp)
        {
            building.Version = provenanceTimestamp;
        }

        private void SetGeometry(BuildingV2 building, string extendedWkbGeometry, BuildingGeometryMethod method)
        {
            var geometry = _wkbReader.Read(extendedWkbGeometry.ToByteArray()) as Polygon;

            building.GeometryMethod = method.Value;
            building.Geometry = geometry?.AsBinary();
        }
    }
}
