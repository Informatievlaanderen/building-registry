namespace BuildingRegistry.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using Newtonsoft.Json;
    using ValueObjects;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabBuildingStatus;

    public static class BuildingSnapshotBuilder
    {
        public static BuildingSnapshot WithBuildingUnitCollection(this BuildingSnapshot snapshot, BuildingUnitCollectionSnapshot buildingUnitCollectionSnapshot)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                buildingUnitCollectionSnapshot,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithImportedTerrainObjectHouseNrIds(this BuildingSnapshot snapshot, IEnumerable<CrabTerrainObjectHouseNumberId> importedCrabTerrainObjectHouseNumberIds)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                importedCrabTerrainObjectHouseNumberIds,
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithActiveHouseNumberIdsByTerrainObjectHouseNr(this BuildingSnapshot snapshot, Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> activeCrabHouseNumberIds)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                activeCrabHouseNumberIds,
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }
        
        public static BuildingSnapshot WithHouseNumberStatusEventsByHouseNumberId(this BuildingSnapshot snapshot, Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>> houseNumberStatusEventsByHouseNumberId)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                houseNumberStatusEventsByHouseNumberId,
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithHouseNumberPositionEventsByHouseNumberId(this BuildingSnapshot snapshot, Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>> houseNumberPositionEventsByHouseNumberId)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                houseNumberPositionEventsByHouseNumberId,
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(
            this BuildingSnapshot snapshot,
            Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>> subaddressEventsByTerrainObjectHouseNr)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                subaddressEventsByTerrainObjectHouseNr,
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithSubaddressPositionEventsBySubaddressId(
            this BuildingSnapshot snapshot,
            Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>> subaddressPositionEvents)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                subaddressPositionEvents,
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithSubaddressStatusEventsBySubaddressId(this BuildingSnapshot snapshot,
            Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>> subaddressStatusEvents)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                subaddressStatusEvents,
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithStatusChronicle(this BuildingSnapshot snapshot, ImportBuildingStatusFromCrab buildingStatusFromCrab)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                new List<BuildingStatusWasImportedFromCrab> { buildingStatusFromCrab.ToLegacyEvent() },
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithGeometryChronicle(this BuildingSnapshot snapshot,
            ImportBuildingGeometryFromCrab buildingGeometriesFromCrab)
        {
            return snapshot.WithGeometryChronicle(
                new List<ImportBuildingGeometryFromCrab>(new List<ImportBuildingGeometryFromCrab>()
                    {buildingGeometriesFromCrab}));
        }

        public static BuildingSnapshot WithGeometryChronicle(this BuildingSnapshot snapshot, IList<ImportBuildingGeometryFromCrab> buildingGeometriesFromCrab)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                buildingGeometriesFromCrab.Select(x => x.ToLegacyEvent()).ToList(),
                snapshot.StatusChronicle,
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithStatus(this BuildingSnapshot snapshot, BuildingStatus? buildingStatus)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                buildingStatus,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithIsRemoved(this BuildingSnapshot snapshot, bool isRemoved)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                isRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot WithLastModificationFromCrab(this BuildingSnapshot snapshot, Modification modification)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                modification);
        }

        public static BuildingSnapshot WithGeometry(this BuildingSnapshot snapshot, BuildingGeometry buildingGeometry)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                buildingGeometry,
                snapshot.Status,
                snapshot.IsComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }

        public static BuildingSnapshot BecameComplete(this BuildingSnapshot snapshot, bool isComplete)
        {
            return new BuildingSnapshot(
                new BuildingId(snapshot.BuildingId),
                snapshot.PersistentLocalId.HasValue ? new PersistentLocalId(snapshot.PersistentLocalId.Value) : null,
                GetGeometry(snapshot),
                snapshot.Status,
                isComplete,
                snapshot.IsRemoved,
                snapshot.GeometryChronicle.ToList(),
                snapshot.StatusChronicle.ToList(),
                GetActiveHouseNumberIdsByTerrainObjectHouseNr(snapshot),
                GetHouseNumberStatusEventsByHouseNumberId(snapshot),
                GetHouseNumberPositionEventsByHouseNumberId(snapshot),
                GetHouseNumberReaddressedEventsByBuildingUnit(snapshot),
                GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(snapshot),
                GetSubaddressStatusEventsBySubaddressId(snapshot),
                GetSubaddressPositionEventsBySubaddressId(snapshot),
                GetSubaddressReaddressedEventsByBuildingUnit(snapshot),
                GetImportedTerrainObjectHouseNumberIds(snapshot),
                snapshot.BuildingUnitCollection,
                snapshot.LastModificationBasedOnCrab);
        }


        public static BuildingSnapshot CreateDefaultSnapshot(BuildingId buildingId)
        {
            return new BuildingSnapshot(
                buildingId,
                null,
                null,
                null,
                false,
                false,
                new List<BuildingGeometryWasImportedFromCrab>(),
                new List<BuildingStatusWasImportedFromCrab>(),
                new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(),
                new Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>>(),
                new Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>>(),
                new Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab>(),
                new Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>>(),
                new Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>>(),
                new Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>>(),
                new Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab>(), new List<CrabTerrainObjectHouseNumberId>(), new BuildingUnitCollectionSnapshot(new List<BuildingUnitSnapshot>(), new Dictionary<BuildingUnitKey, BuildingUnitKey>()), Modification.Insert);
        }

        public static SnapshotContainer Build(
            this BuildingSnapshot snapshot,
            long position,
            JsonSerializerSettings serializerSettings)
        {
            return new SnapshotContainer
            {
                Info = new SnapshotInfo { Position = position, Type = nameof(BuildingSnapshot) },
                Data = JsonConvert.SerializeObject(snapshot, serializerSettings)
            };
        }

        private static BuildingGeometry? GetGeometry(BuildingSnapshot snapshot)
            => snapshot.GeometryMethod.HasValue ? new BuildingGeometry(new ExtendedWkbGeometry(snapshot.GeometryAsHex.ToByteArray()), snapshot.GeometryMethod.Value) : null;

        private static List<CrabTerrainObjectHouseNumberId> GetImportedTerrainObjectHouseNumberIds(BuildingSnapshot snapshot)
            => snapshot.ImportedTerrainObjectHouseNumberIds.Select(x => new CrabTerrainObjectHouseNumberId(x)).ToList();

        private static Dictionary<BuildingUnitKey, SubaddressWasReaddressedFromCrab> GetSubaddressReaddressedEventsByBuildingUnit(BuildingSnapshot snapshot)
            => snapshot.SubaddressReaddressedEventsByBuildingUnit
                .ToDictionary(x => new BuildingUnitKey(x.Key), y => y.Value);

        private static Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>> GetSubaddressPositionEventsBySubaddressId(BuildingSnapshot snapshot)
            => snapshot.SubaddressPositionEventsBySubaddressId
                .ToDictionary(x => new CrabSubaddressId(x.Key), y => y.Value.ToList());

        private static Dictionary<CrabSubaddressId, List<AddressSubaddressStatusWasImportedFromCrab>> GetSubaddressStatusEventsBySubaddressId(BuildingSnapshot snapshot)
            => snapshot.SubaddressStatusEventsBySubaddressId
                .ToDictionary(x => new CrabSubaddressId(x.Key), y => y.Value.ToList());

        private static Dictionary<Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>, List<AddressSubaddressWasImportedFromCrab>> GetSubaddressEventsByTerrainObjectHouseNumberAndHouseNumber(BuildingSnapshot snapshot)
            => snapshot.SubaddressEventsByTerrainObjectHouseNumberAndHouseNumber
                .ToDictionary(x => new Tuple<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>(new CrabTerrainObjectHouseNumberId(x.Key.Item1), new CrabHouseNumberId(x.Key.Item2)), y => y.Value.ToList());

        private static Dictionary<BuildingUnitKey, HouseNumberWasReaddressedFromCrab> GetHouseNumberReaddressedEventsByBuildingUnit(BuildingSnapshot snapshot)
            => snapshot.HouseNumberReaddressedEventsByBuildingUnit
                .ToDictionary(x => new BuildingUnitKey(x.Key), y => y.Value);

        private static Dictionary<AddressId, List<AddressHouseNumberPositionWasImportedFromCrab>> GetHouseNumberPositionEventsByHouseNumberId(BuildingSnapshot snapshot)
            => snapshot.HouseNumberPositionEventsByHouseNumberId
                .ToDictionary(x => new AddressId(x.Key), y => y.Value.ToList());

        private static Dictionary<AddressId, List<AddressHouseNumberStatusWasImportedFromCrab>> GetHouseNumberStatusEventsByHouseNumberId(BuildingSnapshot snapshot)
            => snapshot.HouseNumberStatusEventsByHouseNumberId
                .ToDictionary(x => new AddressId(x.Key), y => y.Value.ToList());

        private static Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId> GetActiveHouseNumberIdsByTerrainObjectHouseNr(BuildingSnapshot snapshot)
            => snapshot.ActiveHouseNumberIdsByTerrainObjectHouseNr
                .ToDictionary(x => new CrabTerrainObjectHouseNumberId(x.Key), y => new CrabHouseNumberId(y.Value));

    }
}
