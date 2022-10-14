namespace BuildingRegistry.Projections.Wms.BuildingV2
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;

    [ConnectedProjectionName("WMS gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen data voor het WMS gebouwregister voorziet.")]
    public class BuildingV2Projections : ConnectedProjection<WmsContext>
    {
        private readonly WKBReader _wkbReader;

        public const string MeasuredByGrbMethod = "IngemetenGRB";
        public const string OutlinedMethod = "Ingeschetst";

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

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, MapMethod(BuildingGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingV2 = new BuildingV2
                {
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.BuildingPersistentLocalId),
                    Status = BuildingStatus.Planned,
                    Version = message.Message.Provenance.Timestamp
                };

                SetGeometry(buildingV2, message.Message.ExtendedWkbGeometry, OutlinedMethod);

                await context.BuildingsV2.AddAsync(buildingV2, ct);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Planned;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.Realized;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.UnderConstruction;
                item.Version = message.Message.Provenance.Timestamp;
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var item = await context.BuildingsV2.FindAsync(message.Message.BuildingPersistentLocalId, cancellationToken: ct);
                item.Status = BuildingStatus.NotRealized;
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

        private void SetGeometry(BuildingV2 building, string extendedWkbGeometry, string method)
        {
            var geometry = _wkbReader.Read(extendedWkbGeometry.ToByteArray()) as Polygon;

            building.GeometryMethod = method;
            building.Geometry = geometry?.AsBinary();
        }
    }
}
