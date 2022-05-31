namespace BuildingRegistry.Legacy.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Legacy.Crab;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("AddressHouseNumberStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisnummerStatus en tblHuisnummerStatus_hist te importeren.")]
    public class AddressHouseNumberStatusWasImportedFromCrab : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<int>, IMessage
    {
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }
        
        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie.")]
        public int TerrainObjectHouseNumberId { get; }
        
        [EventPropertyDescription("CRAB-identificator van de huisnummerstatus.")]
        public int HouseNumberStatusId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }
        
        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }
        
        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }
        
        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public Instant Timestamp { get; }
        
        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }
        
        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")] 
        public CrabModification? Modification { get; }
        
        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }
        
        [EventPropertyDescription("Huisnummerstatus.")]
        public CrabAddressStatus AddressStatus { get; }

        [EventPropertyDescription("Unieke sleutel.")]
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
