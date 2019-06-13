namespace BuildingRegistry.Building.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("SubaddressWasReaddressedFromCrab")]
    [EventDescription("Legacy event om heradressing van subaddressen te importeren.")]
    public class SubaddressWasReaddressedFromCrab
    {
        public int TerrainObjectId { get; }
        public int ReaddressingId { get; }
        public LocalDate BeginDate { get; }
        public int OldTerrainObjectHouseNumberId { get; }
        public string OldAddressNature { get; }
        public int OldSubaddressId { get; }
        public int NewTerrainObjectHouseNumberId { get; }
        public string NewAddressNature { get; }
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
