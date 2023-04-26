namespace BuildingRegistry.Building
{
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
    }
}
