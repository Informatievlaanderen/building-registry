namespace BuildingRegistry.Api.Oslo.Building.V3.Query
{
    using System;
    using BuildingRegistry.Building;
    using NodaTime;

    public sealed class BuildingQueryItem
    {
        public int PersistentLocalId { get; init; }
        public string StatusAsString { get; init; }
        public BuildingStatus Status => BuildingStatus.Parse(StatusAsString);
        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; init; }
        public Instant Version => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
    }
}
