namespace BuildingRegistry.Building.Events
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Newtonsoft.Json;

    [EventName("BuildingSnapshot")]
    [EventSnapshot(nameof(SnapshotContainer) + "<BuildingSnapshot>", typeof(SnapshotContainer))]
    [EventDescription("Snapshot of Building with BuildingUnits")]
    public sealed class BuildingSnapshot
    {
        public int BuildingPersistentLocalId { get; }
        public string BuildingStatus { get; }
        public string ExtendedWkbGeometry { get; }
        public string GeometryMethod { get; }
        public bool IsRemoved { get; }

        public string LastEventHash { get; }
        public ProvenanceData LastProvenanceData { get; }

        public IEnumerable<BuildingUnitData> BuildingUnits { get; }
        public IEnumerable<BuildingUnitData> UnusedCommonBuildingUnits { get; }

        public BuildingSnapshot(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingStatus buildingStatus,
            BuildingGeometry geometry,
            bool isRemoved,
            string lastEventHash,
            ProvenanceData lastProvenanceData,
            IEnumerable<BuildingUnit> buildingUnits,
            IEnumerable<BuildingUnit> unusedCommonBuildingUnits)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingStatus = buildingStatus.Value;
            ExtendedWkbGeometry = geometry.Geometry.ToString();
            GeometryMethod = geometry.Method.Value;
            IsRemoved = isRemoved;
            LastEventHash = lastEventHash;
            LastProvenanceData = lastProvenanceData;
            BuildingUnits = buildingUnits.Select(x => new BuildingUnitData(x)).ToList();
            UnusedCommonBuildingUnits = unusedCommonBuildingUnits.Select(x => new BuildingUnitData(x)).ToList();
        }

        [JsonConstructor]
        private BuildingSnapshot(
            int buildingPersistentLocalId,
            string buildingStatus,
            string extendedWkbGeometry,
            string geometryMethod,
            bool isRemoved,
            string lastEventHash,
            ProvenanceData lastProvenanceData,
            IEnumerable<BuildingUnitData> buildingUnits,
            IEnumerable<BuildingUnitData>? unusedCommonBuildingUnits)
            : this(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                BuildingRegistry.Building.BuildingStatus.Parse(buildingStatus),
                new BuildingGeometry(new ExtendedWkbGeometry(extendedWkbGeometry),
                    BuildingGeometryMethod.Parse(geometryMethod)),
                isRemoved,
                lastEventHash,
                lastProvenanceData,
                [],
                [])
        {
            BuildingUnits = buildingUnits;
            UnusedCommonBuildingUnits = unusedCommonBuildingUnits ?? [];
        }

        public sealed class BuildingUnitData
        {
            public int BuildingUnitPersistentLocalId { get; }

            public string Function { get; }
            public string Status { get; }

            public IEnumerable<int> AddressPersistentLocalIds { get; }

            public string GeometryMethod { get; }
            public string ExtendedWkbGeometry { get; }

            public bool IsRemoved { get; }
            public bool HasDeviation { get; }

            public string LastEventHash { get; }
            public ProvenanceData LastProvenanceData { get; }

            public BuildingUnitData(BuildingUnit buildingUnit)
                :this (buildingUnit.BuildingUnitPersistentLocalId,
                    buildingUnit.Function,
                    buildingUnit.Status,
                    buildingUnit.BuildingUnitPosition,
                    buildingUnit.AddressPersistentLocalIds.Select(x => (int)x),
                    buildingUnit.IsRemoved,
                    buildingUnit.HasDeviation,
                    buildingUnit.LastEventHash,
                    buildingUnit.LastProvenanceData)
            { }

            public BuildingUnitData(
                BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
                BuildingUnitFunction buildingUnitFunction,
                BuildingUnitStatus buildingUnitStatus,
                BuildingUnitPosition buildingUnitPosition,
                IEnumerable<int> addressPersistentLocalIds,
                bool isRemoved,
                bool hasDeviation,
                string lastEventHash,
                ProvenanceData lastProvenanceData)
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
                Function = buildingUnitFunction;
                Status = buildingUnitStatus;
                AddressPersistentLocalIds = addressPersistentLocalIds;
                GeometryMethod = buildingUnitPosition.GeometryMethod.GeometryMethod;
                ExtendedWkbGeometry = buildingUnitPosition.Geometry.ToString();
                IsRemoved = isRemoved;
                HasDeviation = hasDeviation;
                LastEventHash = lastEventHash;
                LastProvenanceData = lastProvenanceData;
            }

            [JsonConstructor]
            private BuildingUnitData(
                int buildingUnitPersistentLocalId,
                string function,
                string status,
                List<int> addressPersistentLocalIds,
                string geometryMethod,
                string extendedWkbGeometry,
                bool isRemoved,
                bool hasDeviation,
                string lastEventHash,
                ProvenanceData lastProvenanceData)
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
                Function = function;
                Status = status;
                AddressPersistentLocalIds = addressPersistentLocalIds;
                GeometryMethod = geometryMethod;
                ExtendedWkbGeometry = extendedWkbGeometry;
                IsRemoved = isRemoved;
                HasDeviation = hasDeviation;
                LastEventHash = lastEventHash;
                LastProvenanceData = lastProvenanceData;
            }
        }
    }
}
