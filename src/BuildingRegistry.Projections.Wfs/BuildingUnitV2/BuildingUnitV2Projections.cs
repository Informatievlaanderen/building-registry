namespace BuildingRegistry.Projections.Wfs.BuildingUnitV2
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
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

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
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
                        Status = MapStatus(BuildingUnitStatus.Parse(buildingUnit.Status)),
                        HasDeviation = false
                    };

                    SetPosition(buildingUnitV2, buildingUnit.ExtendedWkbGeometry, MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)));

                    await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, _) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    unit!.Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                    unit.PositionMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);

                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, _) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    unit!.Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                    unit.PositionMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);

                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, _) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    unit!.Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                    unit.PositionMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);

                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, _) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    var unit = await context.BuildingUnitsV2.FindAsync(buildingUnitPersistentLocalId);
                    unit!.Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());
                    unit.PositionMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);

                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(DoNothing);
            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()),
                    PositionMethod = message.Message.GeometryMethod,
                    Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function)),
                    Version = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    Status = PlannedStatus,
                    HasDeviation = message.Message.HasDeviation
                };

                SetPosition(
                    buildingUnitV2,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV2.AddAsync(buildingUnitV2, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = RealizedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = RealizedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, _) =>
           {
               var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
               unit!.Status = PlannedStatus;

               SetVersion(unit, message.Message.Provenance.Timestamp);
           });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = PlannedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = NotRealizedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });


            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = NotRealizedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = PlannedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = RetiredStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = RealizedStatus;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.IsRemoved = true;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.IsRemoved = true;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                unit!.Status = MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus));
                unit.HasDeviation = message.Message.HasDeviation;
                unit.Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function));
                unit.Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                unit.PositionMethod = MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod));
                unit.IsRemoved = false;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, _) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var commonBuildingUnitV2 = new BuildingUnitV2
                {
                    Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.BuildingUnitPersistentLocalId),
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    Position = (Point)_wkbReader.Read(message.Message.ExtendedWkbGeometry.ToByteArray()),
                    PositionMethod = message.Message.GeometryMethod,
                    Function = MapFunction(BuildingUnitFunction.Common),
                    Version = message.Message.Provenance.Timestamp,
                    IsRemoved = false,
                    Status = MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus)),
                    HasDeviation = message.Message.HasDeviation
                };

                SetPosition(
                    commonBuildingUnitV2,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV2.AddAsync(commonBuildingUnitV2, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetPosition(
                    unit,
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

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = RetiredStatus;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = NotRealizedStatus;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV2.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                unit!.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                unit.Status = MapStatus(BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus));

                SetPosition(
                    unit,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                unit.Function = MapFunction(BuildingUnitFunction.Parse(message.Message.Function));
                unit.HasDeviation = message.Message.HasDeviation;
                unit.IsRemoved = false;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
        }

        private static void SetVersion(BuildingUnitV2 unit, Instant timestamp) => unit.Version = timestamp;

        public static string MapFunction(BuildingUnitFunction function)
            => function == BuildingUnitFunction.Common
                ? GebouweenheidFunctie.GemeenschappelijkDeel.ToString()
                : GebouweenheidFunctie.NietGekend.ToString();

        private void SetPosition(BuildingUnitV2 buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry;
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

        public static string MapStatus(BuildingUnitStatus buildingUnitStatus)
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

        private static Task DoNothing<T>(WfsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
