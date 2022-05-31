namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BuildingUnitId : GuidValueObject<BuildingUnitId>
    {
        public BuildingUnitId([JsonProperty("value")] Guid buildingUnitId) : base(buildingUnitId)
        { }
    }
}
