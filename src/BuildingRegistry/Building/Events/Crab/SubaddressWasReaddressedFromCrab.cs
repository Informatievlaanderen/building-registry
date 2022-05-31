namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("SubaddressWasReaddressedFromCrab")]
    [EventDescription("Legacy event om heradressing van subadressen te importeren.")]
    public class SubaddressWasReaddressedFromCrab : IMessage
    {
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }
        
        [EventPropertyDescription("Identificator van een individuele heradressering.")]
        public int ReaddressingId { get; }
        
        [EventPropertyDescription("Datum waarop de heradressering plaatsvond in werkelijkheid.")]
        public LocalDate BeginDate { get; }
        
        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie vóór hernummering.")]
        public int OldTerrainObjectHouseNumberId { get; }
        
        [EventPropertyDescription("Aard van het adres vóór hernummering.")]
        public string OldAddressNature { get; }
        
        [EventPropertyDescription("CRAB-identificator van het subadres (bus- of appartementsnummer) vóór hernummering.")]
        public int OldSubaddressId { get; }
        
        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie na hernummering.")]
        public int NewTerrainObjectHouseNumberId { get; }
        
        [EventPropertyDescription("Aard van het adres na hernummering.")]
        public string NewAddressNature { get; }
        
        [EventPropertyDescription("CRAB-identificator van het subadres (bus- of appartementsnummer) na hernummering.")]
        public int NewSubaddressId { get; }

        public SubaddressWasReaddressedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabAddressNature oldAddressNature,
            CrabSubaddressId oldSubaddressId,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId,
            CrabAddressNature newAddressNature,
            CrabSubaddressId newSubaddressId)
        {
            TerrainObjectId = terrainObjectId;
            ReaddressingId = readdressingId;
            BeginDate = beginDate;
            OldTerrainObjectHouseNumberId = oldTerrainObjectHouseNumberId;
            OldAddressNature = oldAddressNature;
            OldSubaddressId = oldSubaddressId;
            NewTerrainObjectHouseNumberId = newTerrainObjectHouseNumberId;
            NewAddressNature = newAddressNature;
            NewSubaddressId = newSubaddressId;
        }

        [JsonConstructor]
        private SubaddressWasReaddressedFromCrab(
            int terrainObjectId,
            int readdressingId,
            LocalDate beginDate,
            int oldTerrainObjectHouseNumberId,
            string oldAddressNature,
            int oldSubaddressId,
            int newTerrainObjectHouseNumberId,
            string newAddressNature,
            int newSubaddressId)
            : this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabReaddressingId(readdressingId),
                new ReaddressingBeginDate(beginDate),
                new CrabTerrainObjectHouseNumberId(oldTerrainObjectHouseNumberId),
                new CrabAddressNature(oldAddressNature),
                new CrabSubaddressId(oldSubaddressId),
                new CrabTerrainObjectHouseNumberId(newTerrainObjectHouseNumberId),
                new CrabAddressNature(newAddressNature),
                new CrabSubaddressId(newSubaddressId)) { }
    }
}
