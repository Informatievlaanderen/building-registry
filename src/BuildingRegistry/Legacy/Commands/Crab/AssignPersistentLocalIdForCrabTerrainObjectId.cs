namespace BuildingRegistry.Legacy.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class AssignPersistentLocalIdForCrabTerrainObjectId
    {
        private static readonly Guid Namespace = new Guid("41df4b81-ffad-4af7-860f-dbe2161ce14e");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalIdAssignmentDate AssignmentDate { get; }
        public List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId> BuildingUnitPersistentLocalIds { get; }

        public AssignPersistentLocalIdForCrabTerrainObjectId(
            CrabTerrainObjectId terrainObjectId,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate assignmentDate,
            List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId> buildingUnitPersistentLocalIds)
        {
            TerrainObjectId = terrainObjectId;
            PersistentLocalId = persistentLocalId;
            AssignmentDate = assignmentDate;
            BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignPersistentLocalIdForCrabTerrainObjectId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return PersistentLocalId;
            yield return AssignmentDate;
            yield return BuildingUnitPersistentLocalIds;
        }
    }
}
