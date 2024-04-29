namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Building;

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
