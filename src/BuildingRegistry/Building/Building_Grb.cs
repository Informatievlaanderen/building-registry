namespace BuildingRegistry.Building
{
    using Datastructures;
    using Events;

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
    }
}
