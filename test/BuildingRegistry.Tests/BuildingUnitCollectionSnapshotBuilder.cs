namespace BuildingRegistry.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using NodaTime;
    using ValueObjects;

    public static class BuildingUnitCollectionSnapshotBuilder
    {
        public static BuildingUnitCollectionSnapshot WithBuildingUnits(
            this BuildingUnitCollectionSnapshot buildingUnitCollectionSnapshot,
            List<BuildingUnitSnapshot> buildingUnitSnapshots)
        {
            return new BuildingUnitCollectionSnapshot(buildingUnitSnapshots,
                buildingUnitCollectionSnapshot.ReaddressedKeys
                    .ToDictionary(x => new BuildingUnitKey(x.Key), y => new BuildingUnitKey(y.Value)));
        }

        public static BuildingUnitCollectionSnapshot CreateDefaultSnapshot()
        {
            return new BuildingUnitCollectionSnapshot(
                new List<BuildingUnitSnapshot>(),
                new Dictionary<BuildingUnitKey, BuildingUnitKey>());
        }
    }

    public static class BuildingUnitSnapshotBuilder
    {
        public static BuildingUnitSnapshot WithRemoved(
            this BuildingUnitSnapshot snapshot,
            bool isRemoved = true)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                isRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithRetiredBySelf(
            this BuildingUnitSnapshot snapshot,
            bool isRetiredBySelf = true)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                isRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithRetiredByParent(
            this BuildingUnitSnapshot snapshot,
            bool isRetiredByParent = true)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                isRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithRetiredByBuilding(
            this BuildingUnitSnapshot snapshot,
            bool isRetiredByBuilding = true)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                isRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithPreviousAddressId(
            this BuildingUnitSnapshot snapshot,
            AddressId previousAddressId)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                previousAddressId,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithAddressIds(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<AddressId> addressIds)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                addressIds,
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithStatus(
            this BuildingUnitSnapshot snapshot,
            BuildingUnitStatus status)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                status,
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithHouseNumberStatusChronicle(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<AddressHouseNumberStatusWasImportedFromCrab> houseNumberStatusChronicle)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                houseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithHouseNumberPositions(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<AddressHouseNumberPositionWasImportedFromCrab> houseNumberPositions)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                houseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithSubaddressPositions(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<AddressSubaddressPositionWasImportedFromCrab> subaddressPositions)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                subaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithSubaddressStatusChronicle(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<AddressSubaddressStatusWasImportedFromCrab> subaddressStatusChronicle)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                subaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithFunction(
            this BuildingUnitSnapshot snapshot,
            BuildingUnitFunction function)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                function,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithPosition(
            this BuildingUnitSnapshot snapshot,
            BuildingUnitPosition unitPosition)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                unitPosition,
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot BecameComplete(
            this BuildingUnitSnapshot snapshot, bool isComplete
            )
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                isComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                snapshot.ReaddressedEvents);
        }

        public static BuildingUnitSnapshot WithReaddressedEvents(
            this BuildingUnitSnapshot snapshot,
            IEnumerable<BuildingUnitWasReaddressed> readdressedEvents)
        {
            return new BuildingUnitSnapshot(
                new BuildingId(snapshot.BuildingId),
                new BuildingUnitId(snapshot.BuildingUnitId),
                new BuildingUnitKey(snapshot.BuildingUnitKey),
                BuildingUnitFunction.Parse(snapshot.Function).Value,
                string.IsNullOrEmpty(snapshot.Status) ? null : BuildingUnitStatus.Parse(snapshot.Status),
                snapshot.AddressIds.Select(x => new AddressId(x)),
                snapshot.PreviousAddressId.HasValue ? new AddressId(snapshot.PreviousAddressId.Value) : null,
                string.IsNullOrEmpty(snapshot.BuildingUnitPositionGeometryMethod)
                    ? null
                    : new BuildingUnitPosition(new ExtendedWkbGeometry(snapshot.BuildingUnitPositionAsHex), BuildingUnitPositionGeometryMethod.Parse(snapshot.BuildingUnitPositionGeometryMethod)),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.IsRetiredByBuilding,
                snapshot.IsRetiredByParent,
                snapshot.IsRetiredBySelf,
                snapshot.Version,
                snapshot.HouseNumberStatusChronicle,
                snapshot.SubaddressStatusChronicle,
                snapshot.HouseNumberPositions,
                snapshot.SubaddressPositions,
                readdressedEvents);
        }

        public static BuildingUnitSnapshot CreateDefaultSnapshotFor(BuildingUnitWasAddedToRetiredBuilding buildingUnitWasAdded, BuildingUnitStatus status)
        {
            return CreateDefaultSnapshot(
                    new BuildingId(buildingUnitWasAdded.BuildingId),
                    new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId),
                    new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey),
                    buildingUnitWasAdded.BuildingUnitVersion)
                .WithPreviousAddressId(new AddressId(buildingUnitWasAdded.AddressId))
                .WithStatus(status)
                .WithRetiredByBuilding();
        }

        public static BuildingUnitSnapshot CreateDefaultSnapshotFor(BuildingUnitWasAdded buildingUnitWasAdded)
        {
            return CreateDefaultSnapshot(
                    new BuildingId(buildingUnitWasAdded.BuildingId),
                    new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId),
                    new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey),
                    buildingUnitWasAdded.BuildingUnitVersion)
                .WithAddressIds(new List<AddressId> { new AddressId(buildingUnitWasAdded.AddressId) });
        }

        public static BuildingUnitSnapshot CreateDefaultSnapshotFor(CommonBuildingUnitWasAdded buildingUnitWasAdded)
        {
            return CreateDefaultSnapshot(
                new BuildingId(buildingUnitWasAdded.BuildingId),
                new BuildingUnitId(buildingUnitWasAdded.BuildingUnitId),
                new BuildingUnitKey(buildingUnitWasAdded.BuildingUnitKey),
                buildingUnitWasAdded.BuildingUnitVersion)
                .WithFunction(BuildingUnitFunction.Common);
        }

        public static BuildingUnitSnapshot CreateDefaultSnapshot(
            BuildingId buildingId,
            BuildingUnitId buildingUnitId,
            BuildingUnitKey buildingUnitKey,
            Instant version)
        {
            return new BuildingUnitSnapshot(
                buildingId,
                buildingUnitId,
                buildingUnitKey,
                BuildingUnitFunction.Unknown,
                null,
                new List<AddressId>(),
                null,
                null,
                null,
                false,
                false,
                false,
                false,
                false,
                version,
                new List<AddressHouseNumberStatusWasImportedFromCrab>(),
                new List<AddressSubaddressStatusWasImportedFromCrab>(),
                new List<AddressHouseNumberPositionWasImportedFromCrab>(),
                new List<AddressSubaddressPositionWasImportedFromCrab>(),
                new List<BuildingUnitWasReaddressed>());
        }
    }
}
