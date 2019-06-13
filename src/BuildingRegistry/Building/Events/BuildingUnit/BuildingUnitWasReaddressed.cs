namespace BuildingRegistry.Building.Events
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using System;
    using NodaTime;
    using ValueObjects;

    [EventName("BuildingUnitWasReaddressed")]
    [EventDescription("Gebouweenheid werd geheradresseerd")]
    public class BuildingUnitWasReaddressed
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public Guid OldAddressId { get; }
        public Guid NewAddressId { get; }
        public LocalDate BeginDate { get; }

        public BuildingUnitWasReaddressed(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            AddressId oldAddressId,
            AddressId newAddressId,
            ReaddressingBeginDate beginDate)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            OldAddressId = oldAddressId;
            NewAddressId = newAddressId;
            BeginDate = beginDate;
        }

        [JsonConstructor]
        private BuildingUnitWasReaddressed(
            Guid buildingId,
            Guid buildingUnitId,
            Guid oldAddressId,
            Guid newAddressId,
            LocalDate beginDate)
            : this(
                new BuildingId(buildingId),
                new BuildingUnitId(buildingUnitId),
                new AddressId(oldAddressId),
                new AddressId(newAddressId),
                new ReaddressingBeginDate(beginDate)) { }
    }
}
