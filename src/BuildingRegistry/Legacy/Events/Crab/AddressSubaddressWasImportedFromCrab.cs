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
    [EventName("AddressSubaddressWasImportedFromCrab")]
    [EventDescription("Legacy event om tblSubadres en tblSubadres_hist te importeren voor gemGebouwen.")]
    public class AddressSubaddressWasImportedFromCrab : IMessage
    {
        [EventPropertyDescription("CRAB-identificator van het terreinobject.")]
        public int TerrainObjectId { get; }

        [EventPropertyDescription("CRAB-identificator van de terreinobject-huisnummerrelatie.")]
        public int TerrainObjectHouseNumberId { get; }

        [EventPropertyDescription("CRAB-identificator van het subadres (bus- of appartementsnummer).")]
        public int SubaddressId { get; }

        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }

        [EventPropertyDescription("Aanduiding gebruikt voor het subadres.")]
        public string BoxNumber { get; }

        [EventPropertyDescription("Aard van het subadres.")]
        public string BoxNumberType { get; }

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
