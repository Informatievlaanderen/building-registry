namespace BuildingRegistry.Projections.Wfs.BuildingUnitV2
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("WFS gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het WFS gebouwenregister voorziet.")]
    public class BuildingUnitV2Projections : ConnectedProjection<WfsContext>
    {
        private readonly WKBReader _wkbReader;
        private static readonly string NotRealizedStatus = GebouweenheidStatus.NietGerealiseerd.ToString();
        private static readonly string RetiredStatus = GebouweenheidStatus.Gehistoreerd.ToString();
        private static readonly string PlannedStatus = GebouweenheidStatus.Gepland.ToString();
        private static readonly string RealizedStatus = GebouweenheidStatus.Gerealiseerd.ToString();
        private static readonly string AppointedByAdministratorMethod = PositieGeometrieMethode.AangeduidDoorBeheerder.ToString();
        private static readonly string DerivedFromObjectMethod = PositieGeometrieMethode.AfgeleidVanObject.ToString();

        public BuildingUnitV2Projections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                await context.BuildingUnitsBuildingsV2.AddAsync(new BuildingUnitBuildingItemV2
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    IsRemoved = message.Message.IsRemoved,
                    BuildingRetiredStatus = MapBuildingRetiredStatus(BuildingStatus.Parse(message.Message.BuildingStatus))
                }, ct);

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var buildingUnitV2 = new BuildingUnitV2
                    {
                        Id = PersistentLocalIdHelper.CreateBuildingUnitId(buildingUnit.BuildingUnitPersistentLocalId),
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        Function = MapFunction(BuildingUnitFunction.Parse(buildingUnit.Function)),
                        Version = message.Message.Provenance.Timestamp,
                        IsRemoved = buildingUnit.IsRemoved,
                        Status = MapStatus(BuildingUnitStatus.Parse(buildingUnit.Status))
                    };

                    SetPosition(buildingUnitV2, buildingUnit.ExtendedWkbGeometry, MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)));

                    await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
                }
            });
        }

        private static void SetVersion(BuildingUnitV2 unit, Instant timestamp) => unit.Version = timestamp;

        private static string MapFunction(BuildingUnitFunction function)
            => function == BuildingUnitFunction.Common ? GebouweenheidFunctie.GemeenschappelijkDeel.ToString() : GebouweenheidFunctie.NietGekend.ToString();

        private void SetPosition(BuildingUnitV2 buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry;
        }

        private static string MapGeometryMethod(BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var dictionary = new Dictionary<BuildingUnitPositionGeometryMethod, string>
            {
                {BuildingUnitPositionGeometryMethod.DerivedFromObject, DerivedFromObjectMethod},
                {BuildingUnitPositionGeometryMethod.AppointedByAdministrator, AppointedByAdministratorMethod}
            };

            return dictionary[geometryMethod];
        }

        private static string MapStatus(BuildingUnitStatus buildingUnitStatus)
        {
            var dictionary = new Dictionary<BuildingUnitStatus, string>
            {
                {BuildingUnitStatus.Planned, PlannedStatus},
                {BuildingUnitStatus.Realized, RealizedStatus},
                {BuildingUnitStatus.NotRealized, NotRealizedStatus},
                {BuildingUnitStatus.Retired, RetiredStatus}
            };

            return dictionary[buildingUnitStatus];
        }

        private static BuildingStatus? MapBuildingRetiredStatus(BuildingStatus buildingStatus) =>
            buildingStatus == BuildingStatus.NotRealized || buildingStatus == BuildingStatus.Retired
                ? buildingStatus
                : null;
    }
}
