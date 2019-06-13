namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects.Crab;

    [EventName("AddressHouseNumberStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisnummerStatus en tblHuisnummerStatus_hist te importeren.")]
    public class AddressHouseNumberStatusWasImportedFromCrab : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<int>
    {
        public int TerrainObjectId { get; }
        public int TerrainObjectHouseNumberId { get; }
        public int HouseNumberStatusId { get; }
        public int HouseNumberId { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }
        public CrabAddressStatus AddressStatus { get; }

        public int Key => HouseNumberStatusId;

        public AddressHouseNumberStatusWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            HouseNumberStatusId = houseNumberStatusId;
            HouseNumberId = houseNumberId;
            AddressStatus = addressStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressHouseNumberStatusWasImportedFromCrab(
            int terrainObjectId,
            int terrainObjectHouseNumberId,
            int houseNumberStatusId,
            int houseNumberId,
            CrabAddressStatus addressStatus,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabHouseNumberStatusId(houseNumberStatusId),
                new CrabHouseNumberId(houseNumberId),
                addressStatus,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
