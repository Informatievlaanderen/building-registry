namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("AddressSubaddressWasImportedFromCrab")]
    [EventDescription("Legacy event om tblSubadres en tblSubadres_hist te importeren.")]
    public class AddressSubaddressWasImportedFromCrab
    {
        public int TerrainObjectId { get; }
        public int TerrainObjectHouseNumberId { get; }
        public int SubaddressId { get; }
        public int HouseNumberId { get; }
        public string BoxNumber { get; }
        public string BoxNumberType { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public AddressSubaddressWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            SubaddressId = subaddressId;
            HouseNumberId = houseNumberId;
            BoxNumber = boxNumber;
            BoxNumberType = boxNumberType;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressSubaddressWasImportedFromCrab(
            int terrainObjectId,
            int terrainObjectHouseNumberId,
            int subaddressId,
            int houseNumberId,
            string boxNumber,
            string boxNumberType,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabSubaddressId(subaddressId),
                new CrabHouseNumberId(houseNumberId),
                new BoxNumber(boxNumber),
                new CrabBoxNumberType(boxNumberType),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
