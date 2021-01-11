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
        [EventPropertyDescription("CRAB-identificator van de gebouwstatus.")]
        public int BuildingStatusId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }
        
        [EventPropertyDescription("Gebouwstatus.")]
        public CrabBuildingStatus BuildingStatus { get; }
        
        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }
        
        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }
        
        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; }
        
        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }
        
        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")] 
        public CrabModification? Modification { get; }
        
        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }

        [EventPropertyDescription("Unieke sleutel.")]
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
