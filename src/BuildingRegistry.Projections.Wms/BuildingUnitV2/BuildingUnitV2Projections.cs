namespace BuildingRegistry.Projections.Wms.BuildingUnitV2
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("WMS gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het WMS gebouwregister voorziet.")]
    public class BuildingUnitV2Projections : ConnectedProjection<WmsContext>
    {
        private static readonly string AppointedByAdministratorMethod = PositieGeometrieMethode.AangeduidDoorBeheerder.ToString();
        private static readonly string DerivedFromObjectMethod = PositieGeometrieMethode.AfgeleidVanObject.ToString();

        private readonly WKBReader _wkbReader;

        public BuildingUnitV2Projections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                await context.BuildingUnitBuildingsV2.AddAsync(new BuildingUnitBuildingItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    IsRemoved = message.Message.IsRemoved,
                    BuildingRetiredStatus = MapBuildingRetiredStatus(BuildingStatus.Parse(message.Message.BuildingStatus))
                }, ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    if (buildingUnit.IsRemoved)
                    {
                        continue;
                    }

                    var buildingUnitV2 = new BuildingUnitV2
                    {
                        Id = PersistentLocalIdHelper.CreateBuildingUnitId(buildingUnit.BuildingUnitPersistentLocalId),
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        Function = MapFunction(BuildingUnitFunction.Parse(buildingUnit.Function)),
                        Version = message.Message.Provenance.Timestamp,
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status)
                    };

                    SetPosition(buildingUnitV2, buildingUnit.ExtendedWkbGeometry, MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)));

                    await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                await context.BuildingUnitBuildingsV2.AddAsync(new BuildingUnitBuildingItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    IsRemoved = false,
                    BuildingRetiredStatus = null
                }, ct);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)),
                    Version = message.Message.Provenance.Timestamp,
                    Status = BuildingUnitStatus.Planned
                };

                SetPosition(
                    buildingUnitV2,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });
        }

        private static void SetVersion(BuildingUnitV2 unit, Instant timestamp)
        {
            unit.Version = timestamp;
        }
        public static string MapGeometryMethod(BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var dictionary = new Dictionary<BuildingUnitPositionGeometryMethod, string>
            {
                {BuildingUnitPositionGeometryMethod.DerivedFromObject, DerivedFromObjectMethod},
                {BuildingUnitPositionGeometryMethod.AppointedByAdministrator, AppointedByAdministratorMethod}
            };

            return dictionary[geometryMethod];
        }

        public static string MapFunction(BuildingUnitFunction function)
            => function == BuildingUnitFunction.Common ? "GemeenschappelijkDeel" : "NietGekend";

        private void SetPosition(BuildingUnitV2 buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry.AsBinary();
        }

        private static BuildingStatus? MapBuildingRetiredStatus(BuildingStatus buildingStatus) =>
            buildingStatus == BuildingStatus.NotRealized || buildingStatus == BuildingStatus.Retired
                ? buildingStatus
                : null;
    }
}
