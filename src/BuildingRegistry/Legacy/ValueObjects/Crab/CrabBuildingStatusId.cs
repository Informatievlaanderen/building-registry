namespace BuildingRegistry.Legacy.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class CrabBuildingStatusId : IntegerValueObject<CrabBuildingStatusId>
    {
        public CrabBuildingStatusId([JsonProperty("value")] int buildingStatusId) : base(buildingStatusId) { }
    }
}
