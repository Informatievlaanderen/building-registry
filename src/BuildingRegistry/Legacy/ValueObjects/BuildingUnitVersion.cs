namespace BuildingRegistry.Legacy
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Newtonsoft.Json;
    using NodaTime;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public class BuildingUnitVersion : InstantValueObject<BuildingUnitVersion>
    {
        public BuildingUnitVersion([JsonProperty("value")] Instant versionTimestamp) : base(versionTimestamp) { }
    }
}
