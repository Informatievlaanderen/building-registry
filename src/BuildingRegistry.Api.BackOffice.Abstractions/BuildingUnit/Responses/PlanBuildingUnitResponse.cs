namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Responses
{
    public record PlanBuildingUnitResponse(
        int BuildingUnitPersistentLocalId,
        string LastEventHash);
}
