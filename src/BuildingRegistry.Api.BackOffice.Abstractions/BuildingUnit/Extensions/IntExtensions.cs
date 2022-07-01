namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Extensions
{
    using BuildingRegistry.Building;

    public static class IntExtensions
    {
        public static bool TryGetBuildingIdForBuildingUnit(this int buildingUnitPersistentLocalId, BackOfficeContext backOfficeContext, out BuildingPersistentLocalId buildingPersistentLocalId)
        {
            buildingPersistentLocalId = new BuildingPersistentLocalId(0);

            var buildingUnitBuilding = backOfficeContext.BuildingUnitBuildings
                .Find(buildingUnitPersistentLocalId);

            if (buildingUnitBuilding is null)
            {
                return false;
            }

            buildingPersistentLocalId = new BuildingPersistentLocalId(buildingUnitBuilding.BuildingPersistentLocalId);
            return true;
        }

    }
}
