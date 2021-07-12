namespace BuildingRegistry.Building.DataStructures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Events;
    using Events.Crab;
    using NodaTime;
    using ValueObjects;

    public class BuildingUnitSnapshot
    {
        public Guid BuildingId { get; }
        public Guid BuildingUnitId { get; }
        public BuildingUnitKeyType BuildingUnitKey { get; }
        public string Function { get; }
        public string? Status { get; }
        public IEnumerable<Guid> AddressIds { get; }
        public Guid PreviousAddressId { get; }
        public string? BuildingUnitPositionGeometryMethod { get; }
        public string? BuildingUnitPositionAsHex { get; }
        public int? PersistentLocalId { get; }
        public bool IsComplete { get; }
        public bool IsRemoved { get; }
        public bool IsRetiredByBuilding { get; }
        public bool IsRetiredByParent { get; }
        public bool IsRetiredBySelf { get; }
        public Instant Version { get; }
        public IEnumerable<AddressHouseNumberStatusWasImportedFromCrab> HouseNumberStatusChronicle { get; }
        public IEnumerable<AddressSubaddressStatusWasImportedFromCrab> SubaddressStatusChronicle { get; }
        public IEnumerable<AddressHouseNumberPositionWasImportedFromCrab> HouseNumberPositions { get; }
        public IEnumerable<AddressSubaddressPositionWasImportedFromCrab> SubaddressPositions { get; }
        public IEnumerable<BuildingUnitWasReaddressed> ReaddressedEvents { get; }

        public BuildingUnitSnapshot(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            BuildingUnitFunction function,
            BuildingUnitStatus? status,
            IEnumerable<AddressId> addressIds,
            AddressId previousAddressId,
            BuildingUnitPosition buildingUnitPosition,
            PersistentLocalId persistentLocalId,
            bool isComplete,
            bool isRemoved,
            bool isRetiredByBuilding,
            bool isRetiredByParent,
            bool isRetiredBySelf,
            Instant version,
            IEnumerable<AddressHouseNumberStatusWasImportedFromCrab> houseNumberStatusChronicle,
            IEnumerable<AddressSubaddressStatusWasImportedFromCrab> subaddressStatusChronicle,
            IEnumerable<AddressHouseNumberPositionWasImportedFromCrab> houseNumberPositions,
            IEnumerable<AddressSubaddressPositionWasImportedFromCrab> subaddressPositions,
            IEnumerable<BuildingUnitWasReaddressed> readdressedEvents)
        {
            BuildingId = buildingId;
            BuildingUnitId = buildingUnitId;
            BuildingUnitKey = buildingUnitKey;
            Function = function;
            Status = status;
            AddressIds = addressIds.Select(x => (Guid)x);
            PreviousAddressId = previousAddressId;
            BuildingUnitPositionGeometryMethod = buildingUnitPosition?.GeometryMethod;
            BuildingUnitPositionAsHex = buildingUnitPosition?.Geometry.ToString();
            PersistentLocalId = persistentLocalId;
            IsComplete = isComplete;
            IsRemoved = isRemoved;
            IsRetiredByBuilding = isRetiredByBuilding;
            IsRetiredByParent = isRetiredByParent;
            IsRetiredBySelf = isRetiredBySelf;
            Version = version;
            HouseNumberStatusChronicle = houseNumberStatusChronicle;
            SubaddressStatusChronicle = subaddressStatusChronicle;
            HouseNumberPositions = houseNumberPositions;
            SubaddressPositions = subaddressPositions;
            ReaddressedEvents = readdressedEvents;
        }
    }
}
