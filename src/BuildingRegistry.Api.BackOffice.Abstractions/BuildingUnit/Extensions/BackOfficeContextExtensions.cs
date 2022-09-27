namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Extensions
{
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;

    public static class BackOfficeContextExtensions
    {
        public static BuildingPersistentLocalId GetBuildingIdForBuildingUnit(this BackOfficeContext backOfficeContext, int buildingUnitPersistentLocalId)
        {
            var buildingUnitBuilding = backOfficeContext.BuildingUnitBuildings
                .Find(buildingUnitPersistentLocalId);

            if (buildingUnitBuilding is null)
            {
                throw new BuildingUnitIsNotFoundException();
            }

            return new BuildingPersistentLocalId(buildingUnitBuilding.BuildingPersistentLocalId);
        }
    }
}
