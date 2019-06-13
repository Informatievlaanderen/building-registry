namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("HouseNumberWasReaddressedFromCrab")]
    [EventDescription("Legacy event om heradressing van huisnummers te importeren.")]
    public class HouseNumberWasReaddressedFromCrab
    {
        public int TerrainObjectId { get; }
        public int ReaddressingId { get; }
        public LocalDate BeginDate { get; }
        public int OldTerrainObjectHouseNumberId { get; }
        public string OldAddressNature { get; }
        public int OldHouseNumberId { get; }
        public int NewTerrainObjectHouseNumberId { get; }
        public string NewAddressNature { get; }
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
