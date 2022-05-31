namespace BuildingRegistry.Legacy
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    public class BuildingUnitVersion : InstantValueObject<BuildingUnitVersion>
    {
        public BuildingUnitVersion([JsonProperty("value")] Instant versionTimestamp) : base(versionTimestamp) { }
    }
}
