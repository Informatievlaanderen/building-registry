namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IBuildingGeometries
    {
        ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry);
    }

    public sealed class BuildingGeometryData
    {
        public int BuildingPersistentLocalId { get; init; }
        public BuildingGeometryMethod GeometryMethod { get; init; }
        public Geometry SysGeometry { get; init; }

        private BuildingGeometryData()
        { }

        public BuildingGeometryData(
            int buildingPersistentLocalId,
            BuildingGeometryMethod geometryMethod, Geometry sysGeometry)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            GeometryMethod = geometryMethod;
            SysGeometry = sysGeometry;
        }
    }
}
