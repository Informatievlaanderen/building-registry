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
    public class BuildingGeometryWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>
    {
        public int BuildingGeometryId { get; }
        public int TerrainObjectId { get; }
        public string Geometry { get; }
        public CrabBuildingGeometryMethod BuildingGeometryMethod { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

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
