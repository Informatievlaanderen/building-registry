namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class MigrateBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("6e10f865-b2d9-48e5-a7e5-1cdb9bc817eb");

        public BuildingId BuildingId { get; }
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingPersistentLocalIdAssignmentDate BuildingPersistentLocalIdAssignmentDate { get; }
        public BuildingStatus BuildingStatus { get; }
        public BuildingGeometry BuildingGeometry { get; }
        public bool IsRemoved { get; }
        public List<BuildingUnit> BuildingUnits { get; private set; }

        public Provenance Provenance { get; }

        public MigrateBuilding(
            Legacy.BuildingId buildingId,
            Legacy.PersistentLocalId persistentLocalId,
            Legacy.PersistentLocalIdAssignmentDate assignmentDate,
            Legacy.BuildingStatus buildingStatus,
            Legacy.BuildingGeometry buildingGeometry,
            bool isRemoved,
            List<BuildingUnit> buildingUnits,
            Provenance provenance)
        {
            BuildingId = new BuildingId(buildingId);
            BuildingPersistentLocalId = new BuildingPersistentLocalId(persistentLocalId);
            BuildingPersistentLocalIdAssignmentDate = new BuildingPersistentLocalIdAssignmentDate(assignmentDate);
            BuildingStatus = Legacy.BuildingStatusHelpers.Map(buildingStatus);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(buildingGeometry.Geometry.ToString()),
                BuildingGeometryMethod.Parse(buildingGeometry.Method.ToString()));
            IsRemoved = isRemoved;
            BuildingUnits = buildingUnits;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MigrateLegacyBuilding-{ToString()}");
        
        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingId.ToString();
            yield return BuildingPersistentLocalId.ToString();
            yield return BuildingPersistentLocalIdAssignmentDate.ToString();
            yield return BuildingStatus;
            yield return BuildingGeometry.ToString();
            yield return IsRemoved.ToString();
        }
    }

    public readonly struct BuildingUnit
    {
        public BuildingUnitId BuildingUnitId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public BuildingUnitFunction Function { get; }
        public BuildingUnitStatus Status { get; }
        public List<AddressPersistentLocalId> AddressPersistentLocalIds { get; }
        public BuildingUnitPosition BuildingUnitPosition { get; }
        public bool IsRemoved { get; }

        public BuildingUnit(
            Legacy.BuildingUnitId buildingUnitId,
            Legacy.PersistentLocalId buildingUnitPersistentLocalId,
            Legacy.BuildingUnitFunction function,
            Legacy.BuildingUnitStatus status,
            List<AddressPersistentLocalId> addressPersistentLocalIds,
            Legacy.BuildingUnitPosition buildingUnitPosition,
            bool isRemoved)
        {
            BuildingUnitId = new BuildingUnitId(buildingUnitId);
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            Function = BuildingUnitFunction.Parse(function);
            Status = BuildingUnitStatus.Parse(status);
            AddressPersistentLocalIds = addressPersistentLocalIds;
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(buildingUnitPosition.Geometry.ToString()),
                BuildingUnitPositionGeometryMethod.Parse(buildingUnitPosition.GeometryMethod));
            IsRemoved = isRemoved;
        }
    }
}
