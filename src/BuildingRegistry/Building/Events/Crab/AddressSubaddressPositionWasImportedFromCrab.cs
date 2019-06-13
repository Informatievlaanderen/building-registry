namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("AddressSubaddressPositionWasImportedFromCrab")]
    [EventDescription("Legacy event om tblAdresPositie en tblAdresPositie_hist te importeren.")]
    public class AddressSubaddressPositionWasImportedFromCrab : ICrabEvent, IHasCrabPosition, IHasCrabKey<int>
    {
        public int TerrainObjectId { get; }
        public int TerrainObjectHouseNumberId { get; }
        public int AddressPositionId { get; }
        public int SubaddressId { get; }
        public string AddressNature { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public int Key => AddressPositionId;
        public string AddressPosition { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }

        public AddressSubaddressPositionWasImportedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabAddressPositionId addressPositionId,
            CrabSubaddressId subaddressId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            TerrainObjectId = terrainObjectId;
            TerrainObjectHouseNumberId = terrainObjectHouseNumberId;
            AddressPositionId = addressPositionId;
            SubaddressId = subaddressId;
            AddressPosition = addressPosition;
            AddressPositionOrigin = addressPositionOrigin;
            AddressNature = addressNature;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressSubaddressPositionWasImportedFromCrab(
            int terrainObjectId,
            int terrainObjectHouseNumberId,
            int addressPositionId,
            int subaddressId,
            string addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            string addressNature,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabAddressPositionId(addressPositionId),
                new CrabSubaddressId(subaddressId),
                new WkbGeometry(addressPosition.ToByteArray()),
                addressPositionOrigin,
                new CrabAddressNature(addressNature),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation) { }
    }
}
