namespace BuildingRegistry.Api.Oslo.BuildingUnit.Query
{
    using System;
    using BuildingRegistry.Building;
    using NodaTime;

    public sealed class BuildingUnitQueryItem
    {
        public int BuildingUnitPersistentLocalId { get; init; }
        public string StatusAsString { get; init; }
        public BuildingUnitStatus Status => BuildingUnitStatus.Parse(StatusAsString);
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; init; }
        public Instant Version => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
    }
}
