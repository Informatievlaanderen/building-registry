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
    [EventName("HouseNumberWasReaddressedFromCrab")]
    [EventDescription("Legacy event om heradressing van huisnummers te importeren.")]
    public class HouseNumberWasReaddressedFromCrab : IMessage
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

        [EventPropertyDescription("CRAB-identificator van het huisnummer vóór hernummering.")]
        public int OldHouseNumberId { get; }

        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie na hernummering.")]
        public int NewTerrainObjectHouseNumberId { get; }

        [EventPropertyDescription("Aard van het adres na hernummering.")]
        public string NewAddressNature { get; }

        [EventPropertyDescription("CRAB-identificator van het huisnummer na hernummering.")]
        public int NewHouseNumberId { get; }

        public HouseNumberWasReaddressedFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabReaddressingId readdressingId,
            ReaddressingBeginDate beginDate,
            CrabTerrainObjectHouseNumberId oldTerrainObjectHouseNumberId,
            CrabAddressNature oldAddressNature,
            CrabHouseNumberId oldHouseNumberId,
            CrabTerrainObjectHouseNumberId newTerrainObjectHouseNumberId,
            CrabAddressNature newAddressNature,
            CrabHouseNumberId newHouseNumberId)
        {
            TerrainObjectId = terrainObjectId;
            ReaddressingId = readdressingId;
            BeginDate = beginDate;
            OldTerrainObjectHouseNumberId = oldTerrainObjectHouseNumberId;
            OldAddressNature = oldAddressNature;
            OldHouseNumberId = oldHouseNumberId;
            NewTerrainObjectHouseNumberId = newTerrainObjectHouseNumberId;
            NewAddressNature = newAddressNature;
            NewHouseNumberId = newHouseNumberId;
        }

        [JsonConstructor]
        private HouseNumberWasReaddressedFromCrab(
            int terrainObjectId,
            int readdressingId,
            LocalDate beginDate,
            int oldTerrainObjectHouseNumberId,
            string oldAddressNature,
            int oldHouseNumberId,
            int newTerrainObjectHouseNumberId,
            string newAddressNature,
            int newHouseNumberId)
            : this(
                new CrabTerrainObjectId(terrainObjectId),
                new CrabReaddressingId(readdressingId),
                new ReaddressingBeginDate(beginDate),
                new CrabTerrainObjectHouseNumberId(oldTerrainObjectHouseNumberId),
                new CrabAddressNature(oldAddressNature),
                new CrabHouseNumberId(oldHouseNumberId),
                new CrabTerrainObjectHouseNumberId(newTerrainObjectHouseNumberId),
                new CrabAddressNature(newAddressNature),
                new CrabHouseNumberId(newHouseNumberId)) { }
    }
}
