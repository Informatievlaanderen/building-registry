namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Detail
{
    public class BuildingUnitOsloResponseWithEtag
    {
        public BuildingUnitOsloV3Response BuildingUnitResponse { get; }

        public string? LastEventHash { get; }

        public BuildingUnitOsloResponseWithEtag(BuildingUnitOsloV3Response buildingUnitResponse, string? lastEventHash = null)
        {
            BuildingUnitResponse = buildingUnitResponse;
            LastEventHash = lastEventHash;
        }
    }
}
