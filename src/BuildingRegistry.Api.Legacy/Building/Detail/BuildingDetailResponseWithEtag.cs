namespace BuildingRegistry.Api.Legacy.Building.Detail
{
    public class BuildingDetailResponseWithEtag
    {
        public BuildingDetailResponse BuildingDetailResponse { get; }
        public string? LastEventHash { get; }

        public BuildingDetailResponseWithEtag(BuildingDetailResponse buildingDetailResponse, string? lastEventHash = null)
        {
            BuildingDetailResponse = buildingDetailResponse;
            LastEventHash = lastEventHash;
        }
    }
}
