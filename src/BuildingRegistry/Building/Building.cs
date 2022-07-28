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
                throw new BuildingCannotBePlacedUnderConstructionException(BuildingStatus);
            }

            ApplyChange(new BuildingBecameUnderConstructionV2(BuildingPersistentLocalId));
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
                throw new BuildingCannotBeRealizedException(BuildingStatus);
            }

            ApplyChange(new BuildingWasRealizedV2(BuildingPersistentLocalId));
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
                throw new BuildingStatusPreventsNotRealizeBuildingException();
            }

            ApplyChange(new BuildingWasNotRealizedV2(BuildingPersistentLocalId));
        }

        public void PlanBuildingUnit(
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position,
            BuildingUnitFunction function,
            bool hasDeviation)
        {
            var validStatuses = new[]
                {BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized};

            if (!validStatuses.Contains(BuildingStatus))
            {
                throw new BuildingUnitCannotBePlannedException();
            }

            // validate command
            var finalPosition = positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                ? BuildingGeometry.Center
                : position;

            if (!IsBuildingUnitPositionValidForBuildingGeometry(positionGeometryMethod, finalPosition))
            {
                throw new BuildingUnitOutsideGeometryBuildingException();
            }

            ApplyChange(new BuildingUnitWasPlannedV2(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                positionGeometryMethod,
                finalPosition,
                function,
                hasDeviation));

            AddCommonBuildingUnit(persistentLocalIdGenerator);
        }

        private void AddCommonBuildingUnit(IPersistentLocalIdGenerator persistentLocalIdGenerator)
        {
            if (BuildingStatus != BuildingStatus.Planned
                && BuildingStatus != BuildingStatus.UnderConstruction
                && BuildingStatus != BuildingStatus.Realized)
            {
                return;
            }

            if (_buildingUnits.RequiresCommonBuildingUnit)
            {
                var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(persistentLocalIdGenerator.GenerateNextPersistentLocalId());

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
            }
        }

        private bool IsBuildingUnitPositionValidForBuildingGeometry(
            BuildingUnitPositionGeometryMethod positionGeometryMethod,
            ExtendedWkbGeometry? position)
        {
            if (positionGeometryMethod != BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
            {
                return true;
            }

            return position is not null
                   && BuildingGeometry.Contains(position);
        }

        public void RealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            if(BuildingStatus != BuildingStatus.Realized)
            {
                throw new BuildingStatusPreventsBuildingUnitRealizationException();
            }

            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.Realize();
        }

        public void NotRealizeBuildingUnit(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException(
                    BuildingPersistentLocalId,
                    buildingUnitPersistentLocalId);
            }

            buildingUnit.NotRealize();
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
                throw new InvalidPolygonException();
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
