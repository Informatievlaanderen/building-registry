namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("BuildingGeometryWasImportedFromCrab")]
    [EventDescription("Legacy event om tblgebouwgeometrie en tblgebouwgeometrie_hist te importeren.")]
    public class BuildingGeometryWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>, IMessage
    {
        [EventPropertyDescription("CRAB-identificator van de gebouwgeometrie.")]
        public int BuildingGeometryId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }
        
        [EventPropertyDescription("Gebouwgeometrie.")]
        public string Geometry { get; }
        
        [EventPropertyDescription("Methode gebruikt voor bepalen van de gebouwgeometrie.")]
        public CrabBuildingGeometryMethod BuildingGeometryMethod { get; }
        
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
        public int Key => BuildingGeometryId;

        public BuildingGeometryWasImportedFromCrab(
            CrabBuildingGeometryId buildingGeometryId,
            CrabTerrainObjectId terrainObjectId,
            WkbGeometry geometry,
            CrabBuildingGeometryMethod buildingGeometryMethod,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            BuildingGeometryId = buildingGeometryId;
            TerrainObjectId = terrainObjectId;
            Geometry = geometry?.ToString();
            BuildingGeometryMethod = buildingGeometryMethod;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private BuildingGeometryWasImportedFromCrab(
            int buildingGeometryId,
            int terrainObjectId,
            string geometry,
            CrabBuildingGeometryMethod buildingGeometryMethod,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
            : this(
                new CrabBuildingGeometryId(buildingGeometryId),
                new CrabTerrainObjectId(terrainObjectId),
                new WkbGeometry(geometry),
                buildingGeometryMethod,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
