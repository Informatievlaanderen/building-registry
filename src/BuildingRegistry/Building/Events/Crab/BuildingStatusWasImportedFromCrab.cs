namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects.Crab;

    [EventName("BuildingStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblgebouwstatus en tblgebouwstatus_hist te importeren.")]
    public class BuildingStatusWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>
    {
        public int BuildingStatusId { get; }
        public int TerrainObjectId { get; }
        public CrabBuildingStatus BuildingStatus { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public int Key => BuildingStatusId;

        public BuildingStatusWasImportedFromCrab(
            CrabBuildingStatusId buildingStatusId,
            CrabTerrainObjectId terrainObjectId,
            CrabBuildingStatus buildingStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            BuildingStatusId = buildingStatusId;
            TerrainObjectId = terrainObjectId;
            BuildingStatus = buildingStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private BuildingStatusWasImportedFromCrab(
            int buildingStatusId,
            int terrainObjectId,
            CrabBuildingStatus buildingStatus,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
            : this(
                new CrabBuildingStatusId(buildingStatusId),
                new CrabTerrainObjectId(terrainObjectId),
                buildingStatus,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) {}
    }
}
