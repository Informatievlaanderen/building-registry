namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using NetTopologySuite.Geometries;

    public interface IBuildingGeometries
    {
        ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry);

        ICollection<BuildingGeometryData> GetOverlappingBuildingOutlines(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry);
    }

    public sealed class BuildingGeometryData
    {
        public int BuildingPersistentLocalId { get; init; }
        public string StatusAsString { get; init; }
        public BuildingGeometryMethod GeometryMethod { get; init; }
        public Geometry SysGeometry { get; init; }
        public bool IsRemoved { get; init; }

        private BuildingGeometryData()
        { }

        public BuildingGeometryData(
            int buildingPersistentLocalId,
            BuildingStatus status,
            BuildingGeometryMethod geometryMethod,
            Geometry sysGeometry,
            bool isRemoved)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            StatusAsString = status;
            GeometryMethod = geometryMethod;
            SysGeometry = sysGeometry;
            IsRemoved = isRemoved;
        }
    }
}
