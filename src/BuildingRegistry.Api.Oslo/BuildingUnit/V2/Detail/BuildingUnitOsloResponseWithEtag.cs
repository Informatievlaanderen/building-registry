namespace BuildingRegistry.Api.Oslo.BuildingUnit.V2.Detail
{
    public class BuildingUnitOsloResponseWithEtag
    {
        public BuildingUnitOsloResponse BuildingUnitResponse { get; }

        public string? LastEventHash { get; }

        public BuildingUnitOsloResponseWithEtag(BuildingUnitOsloResponse buildingUnitResponse, string? lastEventHash = null)
        {
            BuildingUnitResponse = buildingUnitResponse;
            LastEventHash = lastEventHash;
        }
    }
}
