namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects.Crab;

    [EventName("AddressSubaddressStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblSubadresStatus en tblSubadresStatus_hist te importeren.")]
    public class AddressSubaddressStatusWasImportedFromCrab : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<int>
    {
        public int TerrainObjectId { get; }
        public int TerrainObjectHouseNumberId { get; }
        public int SubaddressStatusId { get; }
        public int SubaddressId { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }
        public CrabAddressStatus AddressStatus { get; }

        public int Key => SubaddressStatusId;

        public AddressSubaddressStatusWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabSubaddressStatusId subaddressStatusId,
            CrabSubaddressId subaddressId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            SubaddressStatusId = subaddressStatusId;
            SubaddressId = subaddressId;
            AddressStatus = addressStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressSubaddressStatusWasImportedFromCrab(
            int terrainObjectId,
            int terrainObjectHouseNumberId,
            int subaddressStatusId,
            int subaddressId,
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
                new CrabSubaddressStatusId(subaddressStatusId),
                new CrabSubaddressId(subaddressId),
                addressStatus,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
