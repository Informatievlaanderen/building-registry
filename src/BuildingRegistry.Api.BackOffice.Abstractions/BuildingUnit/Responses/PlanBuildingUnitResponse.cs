namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Responses
{
    public record PlanBuildingUnitResponse(
        int BuildingPersistentLocalId,
        int BuildingUnitPersistentLocalId,
        string LastEventHash);
}
