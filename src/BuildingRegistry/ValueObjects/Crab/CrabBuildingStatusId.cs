namespace BuildingRegistry.ValueObjects.Crab
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class CrabBuildingStatusId : IntegerValueObject<CrabBuildingStatusId>
    {
        public CrabBuildingStatusId([JsonProperty("value")]int buildingStatusId) : base(buildingStatusId) { }
    }
}
