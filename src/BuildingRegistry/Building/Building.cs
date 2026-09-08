namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Commands;
    using Events;
    using Exceptions;
    using NetTopologySuite.Geometries;

    public sealed partial class Building : AggregateRootEntity, ISnapshotable
    {
        public static Building MigrateBuilding(
            IBuildingFactory buildingFactory,
            BuildingId buildingId,
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalIdAssignmentDate assignmentDate,
            BuildingStatus buildingStatus,
            BuildingGeometry buildingGeometry,
            bool isRemoved,
            List<Commands.BuildingUnit> buildingUnits)
        {
            var newBuilding = buildingFactory.Create();
            newBuilding.ApplyChange(
                new BuildingWasMigrated(
                    buildingId,
                    buildingPersistentLocalId,
                    assignmentDate,
                    buildingStatus,
                    buildingGeometry,
                    isRemoved,
                    buildingUnits));

            return newBuilding;
        }

        public static Building Plan(
            IBuildingFactory buildingFactory,
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry,
            IBuildingGeometries buildingGeometries)
        {
            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);

            GuardOutline(geometry);
            if (buildingGeometries.GetOverlappingBuildingOutlines(buildingPersistentLocalId, extendedWkbGeometry).Any())
                throw new BuildingGeometryOverlapsWithOutlinedBuildingException();

            var newBuilding = buildingFactory.Create();
            newBuilding.ApplyChange(
                new BuildingWasPlannedV2(
                    buildingPersistentLocalId,
                    extendedWkbGeometry));

            return newBuilding;
        }

        public void PlaceUnderConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.UnderConstruction)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.Planned);

            ApplyChange(new BuildingBecameUnderConstructionV2(BuildingPersistentLocalId));
        }

        public void RealizeConstruction(IBuildingGeometries buildingGeometries)
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Realized)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.UnderConstruction);

            var overlappingBuildings = buildingGeometries.GetOverlappingBuildings(
                BuildingPersistentLocalId,
                BuildingGeometry.Geometry);

            if (overlappingBuildings.Any(x => x.GeometryMethod == BuildingGeometryMethod.MeasuredByGrb))
            {
                throw new BuildingGeometryOverlapsWithMeasuredBuildingException();
            }

            if (overlappingBuildings.Any())
            {
                throw new BuildingGeometryOverlapsWithOutlinedBuildingException();
            }

            ApplyChange(new BuildingWasRealizedV2(BuildingPersistentLocalId));

            foreach (var unit in _buildingUnits)
            {
                unit.RealizeBecauseBuildingWasRealized();
            }
        }

        public void NotRealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.NotRealized)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.Planned, BuildingStatus.UnderConstruction);

            foreach (var unit in _buildingUnits.PlannedBuildingUnits())
            {
                unit.NotRealizeBecauseBuildingWasNotRealized();
            }

            NotRealizeOrRetireCommonBuildingUnit();

            ApplyChange(new BuildingWasNotRealizedV2(BuildingPersistentLocalId));
        }

        public void CorrectBuildingUnderConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Planned)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.UnderConstruction);

            ApplyChange(new BuildingWasCorrectedFromUnderConstructionToPlanned(BuildingPersistentLocalId));
        }

        public void CorrectRealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.UnderConstruction)
            {
                return;
            }

            if (BuildingStatus != BuildingStatus.Realized)
            {
                throw new BuildingHasInvalidStatusException();
            }

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            if (_buildingUnits.RetiredBuildingUnits().Any())
            {
                throw new BuildingHasRetiredBuildingUnitsException();
            }

            foreach (var unit in _buildingUnits)
            {
                unit.CorrectRealizationBecauseBuildingWasCorrected();
            }

            ApplyChange(new BuildingWasCorrectedFromRealizedToUnderConstruction(BuildingPersistentLocalId));
        }

        public void CorrectNotRealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Planned)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.NotRealized);

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            ApplyChange(new BuildingWasCorrectedFromNotRealizedToPlanned(BuildingPersistentLocalId));
        }

        public void RemoveConstruction()
        {
            if (IsRemoved)
            {
                return;
            }

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            foreach (var buildingUnit in _buildingUnits.GetNotRemovedUnits())
            {
                buildingUnit.RemoveBecauseBuildingWasRemoved();
            }

            ApplyChange(new BuildingWasRemovedV2(BuildingPersistentLocalId));
        }

        public void RemoveMeasuredBuilding()
        {
            if (IsRemoved)
            {
                return;
            }

            if (BuildingGeometry.Method != BuildingGeometryMethod.MeasuredByGrb)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            var notRemovedUnits = _buildingUnits.GetNotRemovedUnits().ToList();
            if(notRemovedUnits
               .Any(x => x.Status == BuildingUnitStatus.Realized || x.Status == BuildingUnitStatus.Planned))
            {
                throw new BuildingHasActiveBuildingUnitsException();
            }

            foreach (var notRemovedUnit in notRemovedUnits)
            {
                notRemovedUnit.RemoveBecauseBuildingWasRemoved();
            }

            ApplyChange(new BuildingWasRemovedV2(BuildingPersistentLocalId));
        }

        public void ChangeOutliningConstruction(
            ExtendedWkbGeometry extendedWkbGeometry,
            IBuildingGeometries buildingGeometries)
        {
            GuardRemovedBuilding();

            GuardValidStatusses(BuildingStatus.Planned, BuildingStatus.Realized, BuildingStatus.UnderConstruction);

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            if (BuildingGeometry.Geometry == extendedWkbGeometry)
            {
                return;
            }

            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);
            GuardOutline(geometry);

            if(buildingGeometries.GetOverlappingBuildingOutlines(BuildingPersistentLocalId, extendedWkbGeometry).Any())
                throw new BuildingGeometryOverlapsWithOutlinedBuildingException();

            var newBuildingGeometry = new BuildingGeometry(extendedWkbGeometry, BuildingGeometryMethod.Outlined);
            var plannedOrRealizedBuildingUnits = _buildingUnits.PlannedBuildingUnits()
                .Concat(_buildingUnits.RealizedBuildingUnits())
                .ToList();

            var buildingUnitsOutsideOfBuildingOutlining = plannedOrRealizedBuildingUnits
                .Where(x =>
                    x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                    && !newBuildingGeometry.Contains(x.BuildingUnitPosition.Geometry));

            if (buildingUnitsOutsideOfBuildingOutlining.Any())
            {
                throw new BuildingHasBuildingUnitsOutsideBuildingGeometryException();
            }

            var buildingUnitsWithPositionDerivedFromBuilding = plannedOrRealizedBuildingUnits
                .Where(x => x.BuildingUnitPosition.GeometryMethod ==
                            BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsPosition = buildingUnitsWithPositionDerivedFromBuilding.Any()
                ? newBuildingGeometry.Center
                : null;

            ApplyChange(new BuildingOutlineWasChanged(
                BuildingPersistentLocalId,
                buildingUnitsWithPositionDerivedFromBuilding,
                extendedWkbGeometry,
                buildingUnitsPosition));
        }

        public void ReaddressAddresses(IReadOnlyDictionary<BuildingUnitPersistentLocalId, IReadOnlyList<ReaddressData>> readdresses)
        {
            var buildingUnitReaddresses = readdresses
                .Select(readdressesEntry =>
                {
                    var buildingUnit = _buildingUnits.GetByPersistentLocalId(readdressesEntry.Key);
                    return buildingUnit.BuildBuildingUnitAddressesWereReaddressed(readdressesEntry.Value);
                })
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList();

            if (!buildingUnitReaddresses.Any())
            {
                return;
            }

            ApplyChange(new BuildingBuildingUnitsAddressesWereReaddressed(
                BuildingPersistentLocalId,
                buildingUnitReaddresses,
                readdresses.SelectMany(x => x.Value).Select(x => new AddressRegistryReaddress(x))
            ));
        }

        public void RepairBuilding()
        {
            if (BuildingStatus == BuildingStatus.Planned
               || BuildingStatus == BuildingStatus.UnderConstruction
               || BuildingStatus == BuildingStatus.Realized)
            {
                foreach (var buildingUnit in _buildingUnits.PlannedBuildingUnits())
                    buildingUnit.RepairPosition(BuildingGeometry);

                foreach (var buildingUnit in _buildingUnits.RealizedBuildingUnits())
                    buildingUnit.RepairPosition(BuildingGeometry);
            }
        }

        /// <summary>
        /// Re-expresses the building geometry and every building unit position in Lambert 2008 (EPSG 3812)
        /// for the one-off event store transformation, see ADR 0006.
        /// </summary>
        /// <remarks>
        /// Deliberately unguarded: unlike <see cref="ChangeOutline"/> this is not an edit of the building
        /// but a change of the reference system its geometry is expressed in, and it has to reach every
        /// building the event store holds - removed, not realized and demolished ones included - or the
        /// event store would be left holding both reference systems forever. <see cref="GuardOutline"/> is
        /// not run either: this changes nothing about the shape it already accepted.
        ///
        /// A building whose geometry and positions are already Lambert 2008 applies nothing, which is what
        /// makes re-running the transformation over a stream a no-op instead of a double transform.
        ///
        /// Unused common units are left alone: they only ever leave <c>BuildingWasMigrated</c>, are never
        /// added back to <see cref="BuildingUnits"/>, and are not projected, so their positions are not read
        /// by anything.
        /// </remarks>
        public void TransformToLambert2008()
        {
            var currentGeometry = ReadGeometry(BuildingGeometry.Geometry);

            var newBuildingGeometry = IsLambert2008(currentGeometry)
                ? BuildingGeometry
                : new BuildingGeometry(
                    // Unrounded: a building outline or GRB measurement is a boundary whose vertices carry far
                    // more decimals than a centimetre, and rounding them would move it. See ADR 0006.
                    ExtendedWkbGeometry.Create(currentGeometry.ToReferenceSystem(ExtendedWkbGeometry.SridLambert2008)),
                    BuildingGeometry.Method);

            var derivedBuildingUnits = new List<BuildingUnitPersistentLocalId>();
            var buildingUnitsWhichBecameDerived = new List<BuildingUnitPersistentLocalId>();
            var buildingUnitsWithOwnPosition = new List<(BuildingUnit BuildingUnit, ExtendedWkbGeometry Position)>();

            foreach (var buildingUnit in _buildingUnits)
            {
                var currentPosition = ReadGeometry(buildingUnit.BuildingUnitPosition.Geometry);

                if (IsLambert2008(currentPosition))
                {
                    continue;
                }

                if (buildingUnit.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                {
                    derivedBuildingUnits.Add(buildingUnit.BuildingUnitPersistentLocalId);
                    continue;
                }

                var newPosition = ExtendedWkbGeometry.Create(currentPosition.ToReferenceSystem(
                    ExtendedWkbGeometry.SridLambert2008,
                    GeometryReferenceSystem.PositionRoundingPrecision));

                // A position the transformation pushed out of its building - a rounding artifact, and a rare
                // one - is re-derived rather than left outside, exactly as a geometry change does. A position
                // that was already outside beforehand is left classified as it is: that is not something this
                // transformation caused, and correcting it here would be an edit.
                if (BuildingGeometry.Contains(buildingUnit.BuildingUnitPosition.Geometry)
                    && !newBuildingGeometry.Contains(newPosition))
                {
                    buildingUnitsWhichBecameDerived.Add(buildingUnit.BuildingUnitPersistentLocalId);
                    continue;
                }

                buildingUnitsWithOwnPosition.Add((buildingUnit, newPosition));
            }

            var hasDerivedBuildingUnits = derivedBuildingUnits.Count != 0 || buildingUnitsWhichBecameDerived.Count != 0;

            if (!ReferenceEquals(newBuildingGeometry, BuildingGeometry) || hasDerivedBuildingUnits)
            {
                ApplyChange(new BuildingGeometryCrsWasChanged(
                    BuildingPersistentLocalId,
                    derivedBuildingUnits,
                    buildingUnitsWhichBecameDerived,
                    newBuildingGeometry.Geometry,
                    hasDerivedBuildingUnits ? newBuildingGeometry.Center : null));
            }

            foreach (var (buildingUnit, position) in buildingUnitsWithOwnPosition)
            {
                buildingUnit.TransformPositionToLambert2008(position);
            }
        }

        /// <summary>
        /// Reads a persisted geometry in the reference system its own bytes carry, falling back to Lambert 72
        /// for the SRID-less ones written before the event store wrote EWKB.
        /// </summary>
        /// <remarks>
        /// Qualified: <c>Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology</c> declares a
        /// <c>WKBReaderFactory</c> of its own, and importing that namespace in this file would bind the
        /// simple name to it. GrAr's version throws on SRID-less bytes instead of falling back. See ADR 0006.
        /// </remarks>
        private static Geometry ReadGeometry(ExtendedWkbGeometry extendedWkbGeometry)
        {
            var extendedWkb = extendedWkbGeometry.ToByteArray();

            return BuildingRegistry.WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);
        }

        private static bool IsLambert2008(Geometry geometry) => geometry.SRID == ExtendedWkbGeometry.SridLambert2008;

        private void GuardRemovedBuilding()
        {
            if (IsRemoved)
            {
                throw new BuildingIsRemovedException(BuildingPersistentLocalId);
            }
        }

        private void GuardValidStatusses(params BuildingStatus[] validStatuses)
        {
            if (!validStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
        }

        private static void GuardPolygon(Geometry? geometry)
        {
            if (
                geometry is not Polygon
                || geometry.SRID != ExtendedWkbGeometry.SridLambert72
                || !GeometryValidator.IsValid(geometry))
            {
                throw new PolygonIsInvalidException();
            }
        }

        private static void GuardOutline(Geometry? geometry)
        {
            GuardPolygon(geometry);
            if (geometry!.Area < 1)
            {
                throw new BuildingOutlineIsTooSmallException();
            }
        }

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        #region Snapshot

        public void RequestSnapshot()
        {
            ApplyChange(new BuildingSnapshotWasRequested(BuildingPersistentLocalId));
        }

        public object TakeSnapshot()
        {
            return new BuildingSnapshot(
                BuildingPersistentLocalId,
                BuildingStatus,
                BuildingGeometry,
                IsRemoved,
                LastEventHash,
                LastProvenanceData,
                BuildingUnits,
                UnusedCommonUnits);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
