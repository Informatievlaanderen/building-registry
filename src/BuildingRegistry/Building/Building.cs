namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using BuildingRegistry.Building.Commands;
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
                BuildingUnits,
                UnusedCommonUnits);
        }

        public ISnapshotStrategy Strategy { get; }

        #endregion
    }
}
