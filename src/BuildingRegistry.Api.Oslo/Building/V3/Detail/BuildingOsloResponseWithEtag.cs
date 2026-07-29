namespace BuildingRegistry.Api.Oslo.Building.V3.Detail
{
    public class BuildingOsloResponseWithEtag
    {
        public BuildingOsloV3Response BuildingResponse { get; }
        public string? LastEventHash { get; }

        public BuildingOsloResponseWithEtag(BuildingOsloV3Response buildingResponse, string? lastEventHash = null)
        {
            BuildingResponse = buildingResponse;
            LastEventHash = lastEventHash;
        }
    }
}
