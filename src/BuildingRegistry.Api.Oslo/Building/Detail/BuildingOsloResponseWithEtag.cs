namespace BuildingRegistry.Api.Oslo.Building.Detail
{
    public class BuildingOsloResponseWithEtag
    {
        public BuildingOsloResponse BuildingResponse { get; }
        public string? LastEventHash { get; }

        public BuildingOsloResponseWithEtag(BuildingOsloResponse buildingResponse, string? lastEventHash = null)
        {
            BuildingResponse = buildingResponse;
            LastEventHash = lastEventHash;
        }
    }
}
