namespace BuildingRegistry.Projections.Wms.BuildingUnitV2
{
    using System.Collections.Generic;
    using System.Linq;
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

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
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
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status),
                        HasDeviation = false
                    };

                    SetPosition(buildingUnitV2, buildingUnit.ExtendedWkbGeometry,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)));

                    await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    SetPosition(
                        unit!,
                        message.Message.ExtendedWkbGeometryBuildingUnits!,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message
                             .BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    SetPosition(
                        unit!,
                        message.Message.ExtendedWkbGeometryBuildingUnits!,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message
                             .BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    SetPosition(
                        unit!,
                        message.Message.ExtendedWkbGeometryBuildingUnits!,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    SetPosition(
                        unit!,
                        message.Message.ExtendedWkbGeometryBuildingUnits!,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)),
                    Version = message.Message.Provenance.Timestamp,
                    Status = BuildingUnitStatus.Planned,
                    HasDeviation = message.Message.HasDeviation
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
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.NotRealized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.NotRealized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Retired;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                context.BuildingUnitsV2.Remove(unit);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                context.BuildingUnitsV2.Remove(unit);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var buildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                    HasDeviation = message.Message.HasDeviation,
                    Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)),
                    Version = message.Message.Provenance.Timestamp,
                };

                SetPosition(
                    buildingUnitV2,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var buildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Function = MapFunction(BuildingUnitFunction.Common),
                    Version = message.Message.Provenance.Timestamp,
                    Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                    HasDeviation = message.Message.HasDeviation
                };

                SetPosition(
                    buildingUnitV2,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetPosition(
                    unit!,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitReaddresses.BuildingUnitPersistentLocalId);

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit.Status = BuildingUnitStatus.Retired;
                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                unit.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                unit.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                unit.Function = BuildingUnitFunction.Parse(message.Message.Function);
                unit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                unit.HasDeviation = message.Message.HasDeviation;
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
                { BuildingUnitPositionGeometryMethod.DerivedFromObject, DerivedFromObjectMethod },
                { BuildingUnitPositionGeometryMethod.AppointedByAdministrator, AppointedByAdministratorMethod }
            };

            return dictionary[geometryMethod];
        }

        public static string MapFunction(BuildingUnitFunction function)
            => function == BuildingUnitFunction.Common ? "GemeenschappelijkDeel" : "NietGekend";

        private void SetPosition(BuildingUnitV2 buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point) _wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry.AsBinary();
        }

        private static BuildingStatus? MapBuildingRetiredStatus(BuildingStatus buildingStatus) =>
            buildingStatus == BuildingStatus.NotRealized || buildingStatus == BuildingStatus.Retired
                ? buildingStatus
                : null;
    }
}
