namespace BuildingRegistry.Legacy.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabBuildingGeometryId : IntegerValueObject<CrabBuildingGeometryId>
    {
        public CrabBuildingGeometryId([JsonProperty("value")] int buildingGeometryId) : base(buildingGeometryId) { }
    }
}
