namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;
    using Exceptions;
    using NetTopologySuite.Geometries;

    public partial class Building : AggregateRootEntity, ISnapshotable
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
            newBuilding.ApplyChange(new BuildingWasMigrated(
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
            newBuilding.ApplyChange(new BuildingWasPlannedV2(
                buildingPersistentLocalId,
                extendedWkbGeometry));

            return newBuilding;
        }

        public void ChangeOutlining(ExtendedWkbGeometry extendedWkbGeometry)
        {
            GuardRemovedBuilding();

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidBuildingGeometryMethodException();
            }

            if (BuildingGeometry.Geometry == extendedWkbGeometry)
            {
                return;
            }

            var newBuildingGeometry = new BuildingGeometry(extendedWkbGeometry, BuildingGeometryMethod.Outlined);
            var plannedOrRealizedBuildingUnits = _buildingUnits.PlannedBuildingUnits
                .Concat(_buildingUnits.RealizedBuildingUnits)
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
                .Where(x => x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
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

        public void PlaceUnderConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.UnderConstruction)
            {
                return;
            }

            var invalidStatuses = new List<BuildingStatus>
            {
                BuildingStatus.Retired,
                BuildingStatus.Realized,
                BuildingStatus.NotRealized
            };

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            ApplyChange(new BuildingBecameUnderConstructionV2(BuildingPersistentLocalId));
        }

        public void CorrectBuildingPlaceUnderConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Planned)
            {
                return;
            }

            var invalidStatuses = new List<BuildingStatus>
            {
                BuildingStatus.Retired,
                BuildingStatus.Realized,
                BuildingStatus.NotRealized
            };

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            ApplyChange(new BuildingWasCorrectedFromUnderConstructionToPlanned(BuildingPersistentLocalId));
        }

        public void RealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Realized)
            {
                return;
            }

            var invalidStatuses = new List<BuildingStatus>
            {
                BuildingStatus.Planned,
                BuildingStatus.Retired,
                BuildingStatus.NotRealized
            };

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            ApplyChange(new BuildingWasRealizedV2(BuildingPersistentLocalId));

            foreach (var unit in _buildingUnits)
            {
                unit.RealizeBecauseBuildingWasRealized();
            }
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
                throw new BuildingHasInvalidBuildingGeometryMethodException();
            }

            if (_buildingUnits.RetiredBuildingUnits.Any())
            {
                throw new BuildingHasRetiredBuildingUnitsException();
            }

            foreach (var unit in _buildingUnits)
            {
                unit.CorrectRealizeBecauseBuildingWasCorrected();
            }

            ApplyChange(new BuildingWasCorrectedFromRealizedToUnderConstruction(BuildingPersistentLocalId));
        }

        public void NotRealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.NotRealized)
            {
                return;
            }

            var invalidStatuses = new List<BuildingStatus>
            {
                BuildingStatus.Realized,
                BuildingStatus.Retired
            };

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            foreach (var unit in _buildingUnits.PlannedBuildingUnits)
            {
                unit.NotRealizeBecauseBuildingWasNotRealized();
            }

            ApplyChange(new BuildingWasNotRealizedV2(BuildingPersistentLocalId));
        }

        public void CorrectNotRealizeConstruction()
        {
            GuardRemovedBuilding();

            if (BuildingStatus == BuildingStatus.Planned)
            {
                return;
            }

            if (BuildingStatus != BuildingStatus.NotRealized)
            {
                throw new BuildingHasInvalidStatusException();
            }

            if (BuildingGeometry.Method != BuildingGeometryMethod.Outlined)
            {
                throw new BuildingHasInvalidBuildingGeometryMethodException();
            }

            ApplyChange(new BuildingWasCorrectedFromNotRealizedToPlanned(BuildingPersistentLocalId));
        }

        public void PlanBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position,
            BuildingUnitFunction function,
            bool hasDeviation)
        {
            GuardRemovedBuilding();
            GuardActiveBuilding();

            if (_buildingUnits.HasPersistentLocalId(new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)))
            {
                throw new BuildingUnitPersistentLocalIdAlreadyExistsException();
            }

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            ApplyChange(new BuildingUnitWasPlannedV2(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition,
                function,
                hasDeviation));

            AddOrUpdateStatusCommonBuildingUnit(addCommonBuildingUnit);
        }

        private void GuardActiveBuilding()
        {
            var validStatuses = new[]
                {BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized};

            if (!validStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
        }

        private void AddOrUpdateStatusCommonBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            GuardRemovedBuilding();
            GuardActiveBuilding();

            if (_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || !_buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            if (!_buildingUnits.HasCommonBuildingUnit())
            {
                AddCommonBuildingUnit(addCommonBuildingUnit);
            }
            else
            {
                UpdateStatusCommonBuildingUnit();
            }
        }

        private void AddCommonBuildingUnit(IAddCommonBuildingUnit addCommonBuildingUnit)
        {
            var commonBuildingUnitPersistentLocalId =
                new BuildingUnitPersistentLocalId(addCommonBuildingUnit.GenerateNextPersistentLocalId());

            var commonBuildingUnitStatus = BuildingStatus == BuildingStatus.Realized
                ? BuildingUnitStatus.Realized
                : BuildingUnitStatus.Planned;

            ApplyChange(new CommonBuildingUnitWasAddedV2(
                BuildingPersistentLocalId,
                commonBuildingUnitPersistentLocalId,
                commonBuildingUnitStatus,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                BuildingGeometry.Center,
                hasDeviation: false));

            addCommonBuildingUnit.AddForBuilding(BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId);
        }

        private void UpdateStatusCommonBuildingUnit()
        {
            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit;
            var commonBuildingUnitPosition = commonBuildingUnit.BuildingUnitPosition.Geometry != BuildingGeometry.Center
                ? BuildingGeometry.Center
                : null;

            if ((BuildingStatus == BuildingStatus.Planned || BuildingStatus == BuildingStatus.UnderConstruction)
                && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    commonBuildingUnitPosition));
            }

            if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.NotRealized)
            {
                ApplyChange(new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    commonBuildingUnitPosition));
                ApplyChange(new BuildingUnitWasRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }

            if (BuildingStatus == BuildingStatus.Realized && commonBuildingUnit.Status == BuildingUnitStatus.Retired)
            {
                ApplyChange(new BuildingUnitWasCorrectedFromRetiredToRealized(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId,
                    commonBuildingUnitPosition));
            }
        }

        private void NotRealizeOrRetireCommonBuildingUnit()
        {
            if (!_buildingUnits.HasPlannedOrRealizedCommonBuildingUnit() || _buildingUnits.RequiresCommonBuildingUnit())
            {
                return;
            }

            var commonBuildingUnit = _buildingUnits.CommonBuildingUnit;

            if (commonBuildingUnit.Status == BuildingUnitStatus.Planned)
            {
                ApplyChange(new BuildingUnitWasNotRealizedV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }

            if (commonBuildingUnit.Status == BuildingUnitStatus.Realized)
            {
                ApplyChange(new BuildingUnitWasRetiredV2(
                    BuildingPersistentLocalId,
                    commonBuildingUnit.BuildingUnitPersistentLocalId));
            }
        }

        public void RealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            if (BuildingStatus != BuildingStatus.Realized)
            {
                throw new BuildingHasInvalidStatusException();
            }

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Realize();
        }

        public void CorrectRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.CorrectRealizeBuildingUnit();
        }

        public void NotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.NotRealize();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        public void CorrectNotRealizeBuildingUnit(
            IAddCommonBuildingUnit addCommonBuildingUnit,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            var invalidStatuses = new[]
                {BuildingStatus.NotRealized, BuildingStatus.Retired};

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.CorrectNotRealize(BuildingGeometry);

            AddOrUpdateStatusCommonBuildingUnit(addCommonBuildingUnit);
        }

        public void CorrectPositionBuildingUnit(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            var invalidStatuses = new[]
                {BuildingStatus.NotRealized, BuildingStatus.Retired};

            if (invalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position!;

            if (!BuildingGeometry.Contains(finalPosition))
            {
                throw new BuildingUnitPositionIsOutsideBuildingGeometryException();
            }

            buildingUnit.CorrectPosition(positionGeometryMethod, finalPosition);
        }

        public void RetireBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            GuardRemovedBuilding();

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitIsNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Retire();

            NotRealizeOrRetireCommonBuildingUnit();
        }

        private void GuardRemovedBuilding()
        {
            if (IsRemoved)
            {
                throw new BuildingIsRemovedException(BuildingPersistentLocalId);
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
