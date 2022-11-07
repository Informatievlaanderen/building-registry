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

        private static readonly List<BuildingStatus> ChangeBuildingOutlineInvalidStatuses = new()
        {
            BuildingStatus.NotRealized,
            BuildingStatus.Retired
        };

        public void ChangeOutlining(ExtendedWkbGeometry extendedWkbGeometry)
        {
            GuardRemovedBuilding();

            if (ChangeBuildingOutlineInvalidStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }

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

        private void GuardActiveBuilding()
        {
            var validStatuses = new[]
                {BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized};

            if (!validStatuses.Contains(BuildingStatus))
            {
                throw new BuildingHasInvalidStatusException();
            }
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
