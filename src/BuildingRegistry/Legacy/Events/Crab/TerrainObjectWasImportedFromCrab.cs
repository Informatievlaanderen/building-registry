namespace BuildingRegistry.Legacy.Events.Crab
{
    using System;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Legacy.Crab;
    using Newtonsoft.Json;
    using NodaTime;

    [HideEvent]
    [Obsolete("CRAB events are obsolete.")]
    [EventName("TerrainObjectWasImportedFromCrab")]
    [EventDescription("Legacy event om tblterreinobject en tblterreinobject_hist te importeren voor gemGebouwen.")]
    public class TerrainObjectWasImportedFromCrab : IMessage
    {
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }

        [EventPropertyDescription("GRB-identificator van het terreinobject.")]
        public string IdentifierTerrainObject { get; }

        [EventPropertyDescription("Aard van het terreinobject.")]
        public string TerrainObjectNatureCode { get; }

        [EventPropertyDescription("X-coördinaat van de centroïde van het terreinobject.")]
        public decimal? XCoordinate { get; }

        [EventPropertyDescription("Y-coördinaat van de centroïde van het terreinobject.")]
        public decimal? YCoordinate { get; }

        [EventPropertyDescription("Aard van het gebouw.")]
        public string BuildingNature { get; }

        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? Begin { get; }

        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? End { get; }

        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; }

        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }

        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")]
        public CrabModification? Modification { get; }

        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }

        public TerrainObjectWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabIdentifierTerrainObject identifierTerrainObject,
            CrabTerrainObjectNatureCode terrainObjectNatureCode,
            CrabCoordinate xCoordinate,
            CrabCoordinate yCoordinate,
            CrabBuildingNature buildingNature,
            CrabLifetime crabLifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            IdentifierTerrainObject = identifierTerrainObject;
            TerrainObjectNatureCode = terrainObjectNatureCode;
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;
            BuildingNature = buildingNature;
            Begin = crabLifetime.BeginDateTime;
            End = crabLifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private TerrainObjectWasImportedFromCrab(
            int terrainObjectId,
            string identifierTerrainObject,
            string terrainObjectNatureCode,
            decimal? xCoordinate,
            decimal? yCoordinate,
            string buildingNature,
            LocalDateTime? begin,
            LocalDateTime? end,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabIdentifierTerrainObject(identifierTerrainObject),
                new CrabTerrainObjectNatureCode(terrainObjectNatureCode),
                xCoordinate.HasValue ? new CrabCoordinate(xCoordinate.Value) : null,
                yCoordinate.HasValue ? new CrabCoordinate(yCoordinate.Value) : null,
                new CrabBuildingNature(buildingNature),
                new CrabLifetime(begin, end),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
