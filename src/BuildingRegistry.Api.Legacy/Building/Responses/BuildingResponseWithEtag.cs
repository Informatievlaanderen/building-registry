namespace BuildingRegistry.Api.Legacy.Building.Responses;

public class BuildingResponseWithEtag
{
    public BuildingResponse BuildingResponse { get; }
    public string? LastEventHash { get; }

    public BuildingResponseWithEtag(BuildingResponse buildingResponse, string? lastEventHash = null)
    {
        BuildingResponse = buildingResponse;
        LastEventHash = lastEventHash;
    }
}
