namespace BuildingRegistry.Building
{
    using System.Linq;
    using Datastructures;
    using Events;
    using Exceptions;

    public sealed partial class Building
    {
        public static Building RealizeAndMeasureUnplannedBuilding(
            IBuildingFactory buildingFactory,
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingGrbData buildingGrbData)
        {
            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);

            GuardPolygon(geometry);

            var newBuilding = buildingFactory.Create();
            newBuilding.ApplyChange(
                new UnplannedBuildingWasRealizedAndMeasured(
                    buildingPersistentLocalId,
                    extendedWkbGeometry));

            newBuilding.ApplyChange(
                new BuildingGeometryWasImportedFromGrb(buildingPersistentLocalId, buildingGrbData));

            return newBuilding;
        }

        public void Demolish(BuildingGrbData grbData)
        {
            GuardRemovedBuilding();

            if (BuildingGeometry.Method != BuildingGeometryMethod.MeasuredByGrb)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            if (BuildingStatus == BuildingStatus.Retired)
            {
                return;
            }

            GuardValidStatusses(BuildingStatus.Realized);

            foreach (var unit in _buildingUnits)
            {
                unit.Demolish();
            }

            ApplyChange(new BuildingWasDemolished(BuildingPersistentLocalId));
            ApplyChange(new BuildingGeometryWasImportedFromGrb(BuildingPersistentLocalId, grbData));
        }

        public void MeasureBuilding(
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingGrbData buildingGrbData)
        {
            GuardRemovedBuilding();

            GuardValidStatusses(BuildingStatus.Planned, BuildingStatus.UnderConstruction, BuildingStatus.Realized, BuildingStatus.NotRealized);

            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);

            GuardPolygon(geometry);

            if (BuildingGeometry.Method == BuildingGeometryMethod.MeasuredByGrb)
            {
                return;
            }

            if (BuildingStatus != BuildingStatus.Realized)
            {
                ApplyChange(new BuildingWasRealizedV2(BuildingPersistentLocalId));
            }

            foreach (var unit in _buildingUnits.PlannedBuildingUnits())
            {
                unit.RealizeBecauseBuildingWasRealized();
            }

            var newBuildingGeometry = new BuildingGeometry(extendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb);
            var realizedBuildingUnits = _buildingUnits.RealizedBuildingUnits().ToList();

            var buildingUnitsWithPositionDerivedFromBuilding = realizedBuildingUnits
                .Where(x => x.BuildingUnitPosition.GeometryMethod ==
                            BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsOutsideOfBuildingOutlining = realizedBuildingUnits
                .Where(x =>
                    x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                    && !newBuildingGeometry.Contains(x.BuildingUnitPosition.Geometry))
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsPosition = buildingUnitsWithPositionDerivedFromBuilding.Any() ||
                                        buildingUnitsOutsideOfBuildingOutlining.Any()
                ? newBuildingGeometry.Center
                : null;

            ApplyChange(new BuildingWasMeasured(
                BuildingPersistentLocalId,
                buildingUnitsWithPositionDerivedFromBuilding,
                buildingUnitsOutsideOfBuildingOutlining,
                extendedWkbGeometry,
                buildingUnitsPosition));
            ApplyChange(new BuildingGeometryWasImportedFromGrb(BuildingPersistentLocalId, buildingGrbData));
        }

        public void CorrectBuildingMeasurement(
            ExtendedWkbGeometry extendedWkbGeometry,
            BuildingGrbData buildingGrbData)
        {
            GuardRemovedBuilding();
            GuardValidStatusses(BuildingStatus.Realized);

            if (BuildingGeometry.Method != BuildingGeometryMethod.MeasuredByGrb)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            GuardPolygon(WKBReaderFactory.Create().Read(extendedWkbGeometry));

            if (BuildingGeometry.Geometry == extendedWkbGeometry)
            {
                return;
            }

            var newBuildingGeometry = new BuildingGeometry(extendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb);
            var plannedOrRealizedBuildingUnits = _buildingUnits.PlannedBuildingUnits()
                .Concat(_buildingUnits.RealizedBuildingUnits())
                .ToList();

            var buildingUnitsWithPositionDerivedFromBuilding = plannedOrRealizedBuildingUnits
                .Where(x => x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsOutsideOfBuildingOutlining = plannedOrRealizedBuildingUnits
                .Where(x =>
                    x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                    && !newBuildingGeometry.Contains(x.BuildingUnitPosition.Geometry))
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsPosition =
                buildingUnitsWithPositionDerivedFromBuilding.Any() || buildingUnitsOutsideOfBuildingOutlining.Any()
                    ? newBuildingGeometry.Center
                    : null;

            ApplyChange(new BuildingMeasurementWasCorrected(
                BuildingPersistentLocalId,
                buildingUnitsWithPositionDerivedFromBuilding,
                buildingUnitsOutsideOfBuildingOutlining,
                extendedWkbGeometry,
                buildingUnitsPosition));

            ApplyChange(new BuildingGeometryWasImportedFromGrb(BuildingPersistentLocalId, buildingGrbData));
        }

        public void ChangeMeasurement(ExtendedWkbGeometry extendedWkbGeometry, BuildingGrbData buildingGrbData)
        {
            GuardRemovedBuilding();
            GuardValidStatusses(BuildingStatus.Realized);

            if (BuildingGeometry.Method != BuildingGeometryMethod.MeasuredByGrb)
            {
                throw new BuildingHasInvalidGeometryMethodException();
            }

            if (BuildingGeometry.Geometry == extendedWkbGeometry)
            {
                return;
            }

            var newBuildingGeometry = new BuildingGeometry(extendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb);
            var plannedOrRealizedBuildingUnits = _buildingUnits.PlannedBuildingUnits()
                .Concat(_buildingUnits.RealizedBuildingUnits())
                .ToList();

            var buildingUnitsOutsideOfBuildingMeasurement = plannedOrRealizedBuildingUnits
                .Where(x =>
                    x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.AppointedByAdministrator
                    && !newBuildingGeometry.Contains(x.BuildingUnitPosition.Geometry))
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsWithPositionDerivedFromBuilding = plannedOrRealizedBuildingUnits
                .Where(x => x.BuildingUnitPosition.GeometryMethod == BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Select(x => x.BuildingUnitPersistentLocalId)
                .ToList();

            var buildingUnitsPosition = buildingUnitsOutsideOfBuildingMeasurement.Any() || buildingUnitsWithPositionDerivedFromBuilding.Any()
                ? newBuildingGeometry.Center
                : null;

            ApplyChange(new BuildingMeasurementWasChanged(
                BuildingPersistentLocalId,
                buildingUnitsWithPositionDerivedFromBuilding,
                buildingUnitsOutsideOfBuildingMeasurement,
                extendedWkbGeometry,
                buildingUnitsPosition));
            ApplyChange(new BuildingGeometryWasImportedFromGrb(BuildingPersistentLocalId, buildingGrbData));
        }
    }
}
