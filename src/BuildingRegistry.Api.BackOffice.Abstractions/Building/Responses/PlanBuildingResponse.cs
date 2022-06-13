namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses
{
    public record PlanBuildingResponse(
        int BuildingPersistentLocalId,
        string LastEventHash);
}
