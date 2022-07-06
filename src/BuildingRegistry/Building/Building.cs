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
            if (IsRemoved)
            {
                throw new BuildingIsRemovedException(BuildingPersistentLocalId);
            }

            if (BuildingStatus == BuildingStatus.UnderConstruction)
            {
                return;
            }

            var invalidStates = new List<BuildingStatus>
            {
                BuildingStatus.Retired,
                BuildingStatus.Realized,
                BuildingStatus.NotRealized,
            };

            if (invalidStates.Contains(BuildingStatus))
            {
                throw new BuildingCannotBePlacedUnderConstructionException(BuildingStatus);
            }

            ApplyChange(new BuildingBecameUnderConstructionV2(BuildingPersistentLocalId));
        }

        public void RealizeConstruction()
        {
            if (IsRemoved)
            {
                throw new BuildingIsRemovedException(BuildingPersistentLocalId);
            }

            if (BuildingStatus == BuildingStatus.Realized)
            {
                return;
            }

            var invalidStates = new List<BuildingStatus>
            {
                BuildingStatus.Planned,
                BuildingStatus.Retired,
                BuildingStatus.NotRealized
            };

            if (invalidStates.Contains(BuildingStatus))
            {
                throw new BuildingCannotBeRealizedException(BuildingStatus);
            }

            ApplyChange(new BuildingWasRealizedV2(BuildingPersistentLocalId));
        }

        public void PlanBuildingUnit(PlanBuildingUnit command)
        {
            // validate command
            var position = command.Position ?? BuildingGeometry.Center;

            if (command.HasDeviation)
            {
                ApplyChange(new DeviatedBuildingUnitWasPlanned(
                    command.BuildingPersistentLocalId,
                    command.BuildingUnitPersistentLocalId,
                    command.PositionGeometryMethod,
                    position,
                    command.Function));
            }
            else
            {
                ApplyChange(new BuildingUnitWasPlannedV2(
                    command.BuildingPersistentLocalId,
                    command.BuildingUnitPersistentLocalId,
                    command.PositionGeometryMethod,
                    position,
                    command.Function));
            }
        }

        public void RealizeBuildingUnit(RealizeBuildingUnit command)
        {
            // validate command
            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == command.BuildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException(
                    command.BuildingPersistentLocalId,
                    command.BuildingUnitPersistentLocalId);
            }

            if (buildingUnit.IsRemoved)
            {
                throw new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId);
            }

            if (buildingUnit.Status == BuildingUnitStatus.Realized)
            {
                return;
            }

            var invalidStatusses = new List<BuildingUnitStatus>
            {
                BuildingUnitStatus.Retired,
                BuildingUnitStatus.NotRealized
            };

            if (invalidStatusses.Contains(buildingUnit.Status))
            {
                throw new BuildingUnitCannotBeRealizedException(buildingUnit.Status);
            }

            ApplyChange(new BuildingUnitWasRealizedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId));
        }

        private static void GuardPolygon(Geometry? geometry)
        {
            if (
                geometry == null
                || geometry is not Polygon
                || geometry.SRID != ExtendedWkbGeometry.SridLambert72
                || !GeometryValidator.IsValid(geometry))
            {
                throw new InvalidPolygonException();
            }
        }
        
        #region Metadata
        protected override void BeforeApplyChange(object @event)
        {
            new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }
        #endregion

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
    }
}
