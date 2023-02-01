namespace BuildingRegistry.Api.Legacy.Abstractions.BuildingUnit.Responses;

public class BuildingUnitResponseWithEtag
{
    public BuildingUnitResponse BuildingUnitResponse { get; }
    public string? LastEventHash { get; }

    public BuildingUnitResponseWithEtag(BuildingUnitResponse buildingUnitResponse, string? lastEventHash = null)
    {
        BuildingUnitResponse = buildingUnitResponse;
        LastEventHash = lastEventHash;
    }
}
