namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
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
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);

            GuardPolygon(geometry);

            var newBuilding = buildingFactory.Create();
            newBuilding.ApplyChange(
                new BuildingWasPlannedV2(
                    buildingPersistentLocalId,
                    extendedWkbGeometry));

            return newBuilding;
        }

        public static Building MergeBuildings(
            IBuildingFactory buildingFactory,
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry,
            List<Building> buildingsToMerge)
        {
            if (buildingsToMerge.Count <= 1)
                throw new BuildingMergerNeedsMoreThanOneBuildingException();

            if (buildingsToMerge.Count > BuildingMergerHasTooManyBuildingsException.MaxNumberOfBuildingsToMerge)
                throw new BuildingMergerHasTooManyBuildingsException();

            foreach (var building in buildingsToMerge)
            {
                if (building.BuildingStatus != BuildingStatus.Realized)
                    throw new BuildingToMergeHasInvalidStatusException();

                if (building.BuildingGeometry.Method != BuildingGeometryMethod.MeasuredByGrb)
                    throw new BuildingToMergeHasInvalidGeometryMethodException();
            }

            var buildingGeometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);
            GuardPolygon(buildingGeometry);

            var newBuilding = buildingFactory.Create();
            newBuilding.ApplyChange(
                new BuildingMergerWasRealized(
                    buildingPersistentLocalId,
                    extendedWkbGeometry,
                    buildingsToMerge.Select(x => x.BuildingPersistentLocalId)));

            //TODO: transfer buildingunits to new building

            foreach (var buildingToMerge in buildingsToMerge)
            {
                var buildingUnitsToTransfer =
                    buildingToMerge._buildingUnits.PlannedBuildingUnits().Concat(buildingToMerge._buildingUnits.RealizedBuildingUnits());

                foreach (var buildingUnit in buildingUnitsToTransfer.Where(x => x.Function != BuildingUnitFunction.Common))
                {
                    var geometryMethod = buildingUnit.BuildingUnitPosition.GeometryMethod;

                    if (!newBuilding.BuildingGeometry.Contains(buildingUnit.BuildingUnitPosition.Geometry))
                    {
                        geometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                    }

                    var geometryPosition = buildingUnit.BuildingUnitPosition.Geometry;

                    if (geometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                    {
                        geometryPosition = newBuilding.BuildingGeometry.Center;
                    }

                    newBuilding.ApplyChange(new BuildingUnitWasTransferred(
                        buildingPersistentLocalId,
                        buildingUnit,
                        buildingToMerge.BuildingPersistentLocalId,
                        new BuildingUnitPosition(geometryPosition, geometryMethod)));

                    newBuilding.EnsureCommonBuildingUnit(addCommonBuildingUnit);
                }

                if (buildingToMerge._buildingUnits.HasCommonBuildingUnit())
                {
                    var commonBuildingUnitToMerge = buildingToMerge._buildingUnits.CommonBuildingUnit();
                    var newCommonBuildingUnit = newBuilding._buildingUnits.CommonBuildingUnit();

                    foreach (var addressToTransfer in commonBuildingUnitToMerge.AddressPersistentLocalIds)
                    {
                        newBuilding._buildingUnits.CommonBuildingUnit();
                        newBuilding.ApplyChange(new BuildingUnitAddressWasAttachedV2(
                            buildingPersistentLocalId,
                            newCommonBuildingUnit.BuildingUnitPersistentLocalId,
                            addressToTransfer
                        ));
                    }
                }
            }

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

        public void RealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Realized)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.UnderConstruction);

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

        public void ChangeOutliningConstruction(ExtendedWkbGeometry extendedWkbGeometry)
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

        #region Metadata

        protected override void BeforeApplyChange(object @event)
        {
            _ = new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }

        #endregion

        #region Snapshot

        public object TakeSnapshot()
        {
            return new BuildingSnapshot(
                BuildingPersistentLocalId,
                BuildingStatus,
                BuildingGeometry,
                IsRemoved,
                LastEventHash,
                LastProvenanceData,
                BuildingUnits);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
