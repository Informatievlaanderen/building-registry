namespace BuildingRegistry.Projections.Wms.BuildingUnitV3
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NodaTime;

    /// <summary>
    /// The Lambert 2008 (EPSG 3812) counterpart of <see cref="BuildingUnitV2.BuildingUnitV2Projections"/>,
    /// produced mechanically from it so the two stay identical apart from the reference system. See ADR 0005.
    /// </summary>
    [ConnectedProjectionName("WMS gebouweenheden (v3, Lambert 2008)")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data in Lambert 2008 voor het WMS gebouwregister voorziet.")]
    public class BuildingUnitV3Projections : ConnectedProjection<WmsContext>
    {
        private static readonly string AppointedByAdministratorMethod = PositieGeometrieMethode.AangeduidDoorBeheerder.ToString();
        private static readonly string DerivedFromObjectMethod = PositieGeometrieMethode.AfgeleidVanObject.ToString();

        /// <summary>
        /// Positions are persisted at centimetre precision, which is what the Lambert transform is
        /// accurate to. See ADR 0005.
        /// </summary>
        private const int PositionCoordinateDecimals = 2;

        public BuildingUnitV3Projections()
        {
            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    if (buildingUnit.IsRemoved)
                    {
                        continue;
                    }

                    var buildingUnitV3 = new BuildingUnitV3
                    {
                        Id = PersistentLocalIdHelper.CreateBuildingUnitId(buildingUnit.BuildingUnitPersistentLocalId),
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        Function = MapFunction(BuildingUnitFunction.Parse(buildingUnit.Function)),
                        Version = message.Message.Provenance.Timestamp,
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status),
                        HasDeviation = false
                    };

                    SetPosition(buildingUnitV3, buildingUnit.ExtendedWkbGeometry,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)));

                    await context.BuildingUnitsV3.AddAsync(buildingUnitV3, ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    var unit = await context.BuildingUnitsV3.FindAsync(buildingUnitPersistentLocalId);
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
                    var unit = await context.BuildingUnitsV3.FindAsync(buildingUnitPersistentLocalId);
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
                    var unit = await context.BuildingUnitsV3.FindAsync(buildingUnitPersistentLocalId);
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
                    var unit = await context.BuildingUnitsV3.FindAsync(buildingUnitPersistentLocalId);
                    SetPosition(
                        unit!,
                        message.Message.ExtendedWkbGeometryBuildingUnits!,
                        MapGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
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
                var buildingUnitV3 = new BuildingUnitV3
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
                    buildingUnitV3,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV3.AddAsync(buildingUnitV3, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.NotRealized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.NotRealized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Planned;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Retired;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.Status = BuildingUnitStatus.Realized;

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                context.BuildingUnitsV3.Remove(unit);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                context.BuildingUnitsV3.Remove(unit);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                var buildingUnitV3 = new BuildingUnitV3
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
                    buildingUnitV3,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV3.AddAsync(buildingUnitV3, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = true;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.HasDeviation = false;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var buildingUnitV3 = new BuildingUnitV3
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
                    buildingUnitV3,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                await context.BuildingUnitsV3.AddAsync(buildingUnitV3, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetPosition(
                    unit!,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    var unit = await context.BuildingUnitsV3.FindAsync(buildingUnitReaddresses.BuildingUnitPersistentLocalId);

                    SetVersion(unit!, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);

                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit.Status = BuildingUnitStatus.Retired;
                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(unit!, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                var unit = await context.BuildingUnitsV3.FindAsync(message.Message.BuildingUnitPersistentLocalId);
                unit!.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;

                SetPosition(
                    unit,
                    message.Message.ExtendedWkbGeometry,
                    MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod)));

                unit.Function = BuildingUnitFunction.Parse(message.Message.Function);
                unit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                unit.HasDeviation = message.Message.HasDeviation;
                SetVersion(unit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
        }

        private static void SetVersion(BuildingUnitV3 unit, Instant timestamp)
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

        private static void SetPosition(BuildingUnitV3 buildingUnit, string extendedWkbPosition, string method)
        {
            buildingUnit.PositionMethod = method;
            buildingUnit.Position = ParsePosition(extendedWkbPosition);
        }

        /// <summary>
        /// Version 3 stores Lambert 2008 (EPSG 3812) and nothing else, whichever reference system the
        /// event store persists. The bytes stored are plain WKB and carry no SRID of their own: the
        /// [wms].[BuildingUnitsV3] computed column labels them 3812, and this is what keeps that label
        /// honest. Once the event store holds Lambert 2008 the transform becomes a no-op. See ADR 0005.
        /// </summary>
        private static byte[] ParsePosition(string extendedWkbPosition)
        {
            var extendedWkb = extendedWkbPosition.ToByteArray();
            var position = (Point)WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);

            // Rounds only when it actually transforms, so a position already in Lambert 2008 is stored
            // exactly as persisted. The transform is accurate to the centimetre positions are kept at.
            return
                position.IsLambert08()
                    ? position.AsBinary()
                    : ((Point)position.EnsureLambert08(PositionCoordinateDecimals))
                        .AsBinary(); //asbinary is a must here since we are using a WKB and not EWKB
        }

        private static Task DoNothing<T>(WmsContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
