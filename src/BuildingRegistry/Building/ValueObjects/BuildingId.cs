namespace BuildingRegistry.Building
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;

    public class BuildingId : GuidValueObject<BuildingId>
    {
        public BuildingId([JsonProperty("value")] Guid buildingId) : base(buildingId) { }
    }
}
