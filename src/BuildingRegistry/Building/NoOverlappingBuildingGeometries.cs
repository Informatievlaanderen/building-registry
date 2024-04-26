namespace BuildingRegistry.Building
{
    using System.Collections.Generic;

    public class NoOverlappingBuildingGeometries : IBuildingGeometries
    {
        public ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            return new List<BuildingGeometryData>();
        }
    }
}
