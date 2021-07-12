namespace BuildingRegistry.Building.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Crab;
    using DataStructures;
    using Newtonsoft.Json;
    using ValueObjects;

    [EventName("BuildingSnapshot")]
    [EventDescription("Snapshot of Building")]
    public class BuildingSnapshot
    {
        public Guid BuildingId { get; }
        public int? PersistentLocalId { get; }
        public string? GeometryAsHex { get; }
        public BuildingGeometryMethod? GeometryMethod { get; }
        public BuildingStatus? Status { get; }
        public bool IsComplete { get; }
        public bool IsRemoved { get; }
        public IEnumerable<BuildingGeometryWasImportedFromCrab> GeometryChronicle { get; }
        public IEnumerable<BuildingStatusWasImportedFromCrab> StatusChronicle { get; }
        public BuildingUnitCollectionSnapshot BuildingUnitCollection { get; }

        public Dictionary<int, int> ActiveHouseNumberIdsByTerrainObjectHouseNr { get; }

        public Dictionary<Tuple<int, int>, IEnumerable<AddressSubaddressWasImportedFromCrab>> SubaddressEventsByTerrainObjectHouseNumberAndHouseNumber { get; }
        public Dictionary<int, IEnumerable<AddressSubaddressStatusWasImportedFromCrab>> SubaddressStatusEventsBySubaddressId { get; }
        public Dictionary<int, IEnumerable<AddressSubaddressPositionWasImportedFromCrab>> SubaddressPositionEventsBySubaddressId { get; }

        public Dictionary<Guid, IEnumerable<AddressHouseNumberStatusWasImportedFromCrab>> HouseNumberStatusEventsByHouseNumberId { get; }
        public Dictionary<Guid, IEnumerable<AddressHouseNumberPositionWasImportedFromCrab>> HouseNumberPositionEventsByHouseNumberId { get; }

        public Dictionary<BuildingUnitKeyType, HouseNumberWasReaddressedFromCrab> HouseNumberReaddressedEventsByBuildingUnit { get; }
        public Dictionary<BuildingUnitKeyType, SubaddressWasReaddressedFromCrab> SubaddressReaddressedEventsByBuildingUnit { get; }
        public IEnumerable<int> ImportedTerrainObjectHouseNumberIds { get; }

        public Modification LastModificationBasedOnCrab { get; }

        public BuildingSnapshot(
            BuildingId buildingId,
            PersistentLocalId? persistentLocalId,
            BuildingGeometry? buildingGeometry,
            BuildingStatus? buildingStatus,
            bool isComplete,
            bool isRemoved,
            IEnumerable<BuildingGeometryWasImportedFromCrab> geometryChronicle,
            IEnumerable<BuildingStatusWasImportedFromCrab> statusChronicle,
            Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> activeHouseNumberIdsByTerrainObjectHouseNr,
            Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>> subaddressEventsByTerrainObjectHouseNumberAndHouseNumber,
            Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>> subaddressStatusEventsBySubaddressId,
            Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>> subaddressPositionEventsBySubaddressId,
            Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>> houseNumberStatusEventsByHouseNumberId,
            Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>> houseNumberPositionEventsByHouseNumberId,
            Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab> houseNumberReaddressedEventsByBuildingUnit,
            Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab> subaddressReaddressedEventsByBuildingUnit,
            IEnumerable<CrabTerrainObjectHouseNumberId> importedTerrainObjectHouseNumberIds,
            BuildingUnitCollectionSnapshot buildingUnitCollection,
            Modification lastModificationBasedOnCrab)
        {
            BuildingId = buildingId;

            if (persistentLocalId is not null)
                PersistentLocalId = persistentLocalId;

            GeometryAsHex = buildingGeometry?.Geometry.ToString();
            GeometryMethod = buildingGeometry?.Method;
            Status = buildingStatus;
            IsComplete = isComplete;
            IsRemoved = isRemoved;
            GeometryChronicle = geometryChronicle;
            StatusChronicle = statusChronicle;

            ActiveHouseNumberIdsByTerrainObjectHouseNr = activeHouseNumberIdsByTerrainObjectHouseNr.ToDictionary(
                x => (int)x.Key,
                y => (int)y.Value);

            SubaddressEventsByTerrainObjectHouseNumberAndHouseNumber = subaddressEventsByTerrainObjectHouseNumberAndHouseNumber.ToDictionary(
                x => new Tuple<int, int>(x.Key.Item1, x.Key.Item2),
                y => y.Value.AsEnumerable());
            SubaddressStatusEventsBySubaddressId = subaddressStatusEventsBySubaddressId.ToDictionary(
                x => (int)x.Key,
                y => y.Value.AsEnumerable());
            SubaddressPositionEventsBySubaddressId = subaddressPositionEventsBySubaddressId.ToDictionary(
                x => (int)x.Key,
                y => y.Value.AsEnumerable());

            HouseNumberStatusEventsByHouseNumberId = houseNumberStatusEventsByHouseNumberId.ToDictionary(
                x => (Guid)x.Key,
                y => y.Value.AsEnumerable());
            HouseNumberPositionEventsByHouseNumberId = houseNumberPositionEventsByHouseNumberId.ToDictionary(
                x => (Guid)x.Key,
                y => y.Value.AsEnumerable());

            HouseNumberReaddressedEventsByBuildingUnit = houseNumberReaddressedEventsByBuildingUnit.ToDictionary(
                x => (BuildingUnitKeyType)x.Key,
                y => y.Value);
            SubaddressReaddressedEventsByBuildingUnit = subaddressReaddressedEventsByBuildingUnit.ToDictionary(
                x => (BuildingUnitKeyType)x.Key,
                y => y.Value);
            ImportedTerrainObjectHouseNumberIds = importedTerrainObjectHouseNumberIds.Select(x => (int)x);

            BuildingUnitCollection = buildingUnitCollection;

            LastModificationBasedOnCrab = lastModificationBasedOnCrab;
        }

        [JsonConstructor]
        private BuildingSnapshot(
            Guid buildingId,
            int? persistentLocalId,
            string? geometryAsHex,
            BuildingGeometryMethod? geometryMethod,
            BuildingStatus? status,
            bool isComplete,
            bool isRemoved,
            IEnumerable<BuildingGeometryWasImportedFromCrab> geometryChronicle,
            IEnumerable<BuildingStatusWasImportedFromCrab> statusChronicle,
            Dictionary<int, int> activeHouseNumberIdsByTerrainObjectHouseNr,
            Dictionary<Tuple<int, int>, IEnumerable<AddressSubaddressWasImportedFromCrab>>
                subaddressEventsByTerrainObjectHouseNumberAndHouseNumber,
            Dictionary<int, IEnumerable<AddressSubaddressStatusWasImportedFromCrab>> subaddressStatusEventsBySubaddressId,
            Dictionary<int, IEnumerable<AddressSubaddressPositionWasImportedFromCrab>> subaddressPositionEventsBySubaddressId,
            Dictionary<Guid, IEnumerable<AddressHouseNumberStatusWasImportedFromCrab>> houseNumberStatusEventsByHouseNumberId,
            Dictionary<Guid, IEnumerable<AddressHouseNumberPositionWasImportedFromCrab>> houseNumberPositionEventsByHouseNumberId,
            Dictionary<BuildingUnitKeyType, HouseNumberWasReaddressedFromCrab> houseNumberReaddressedEventsByBuildingUnit,
            Dictionary<BuildingUnitKeyType, SubaddressWasReaddressedFromCrab> subaddressReaddressedEventsByBuildingUnit,
            IEnumerable<int> importedTerrainObjectHouseNumberIds,
            BuildingUnitCollectionSnapshot buildingUnitCollection,
            Modification lastModificationBasedOnCrab)
            : this(
                new BuildingId(buildingId),
                persistentLocalId.HasValue ? new PersistentLocalId(persistentLocalId.Value) : null,
                geometryMethod.HasValue
                    ? new BuildingGeometry(new ExtendedWkbGeometry(geometryAsHex), geometryMethod.Value)
                    : null,
                status,
                isComplete,
                isRemoved,
                geometryChronicle,
                statusChronicle,
                activeHouseNumberIdsByTerrainObjectHouseNr.ToDictionary(
                    x => new CrabTerrainObjectHouseNumberId(x.Key),
                    y => new CrabHouseNumberId(y.Value)),

                subaddressEventsByTerrainObjectHouseNumberAndHouseNumber.ToDictionary(
                    x => new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(
                        new CrabTerrainObjectHouseNumberId(x.Key.Item1), new CrabHouseNumberId(x.Key.Item2)),
                    y => y.Value.ToList()),
                subaddressStatusEventsBySubaddressId.ToDictionary(
                    x => new CrabSubaddressId(x.Key),
                    y => y.Value.ToList()),
                subaddressPositionEventsBySubaddressId.ToDictionary(
                    x => new CrabSubaddressId(x.Key),
                    y => y.Value.ToList()),

                houseNumberStatusEventsByHouseNumberId.ToDictionary(
                    x => new AddressId(x.Key),
                    y => y.Value.ToList()),
                houseNumberPositionEventsByHouseNumberId.ToDictionary(
                    x => new AddressId(x.Key),
                    y => y.Value.ToList()),

                houseNumberReaddressedEventsByBuildingUnit.ToDictionary(
                    x => new BuildingUnitKey(x.Key),
                    y => y.Value),
                subaddressReaddressedEventsByBuildingUnit.ToDictionary(
                    x => new BuildingUnitKey(x.Key),
                    y => y.Value),
                importedTerrainObjectHouseNumberIds.Select(x => new CrabTerrainObjectHouseNumberId(x)),
                buildingUnitCollection,
                lastModificationBasedOnCrab)
        { }
    }
}
