namespace BuildingRegistry.Building.Commands.Crab
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ValueObjects;

    public class AssignOsloIdForCrabTerrainObjectId
    {
        private static readonly Guid Namespace = new Guid("41df4b81-ffad-4af7-860f-dbe2161ce14e");

        public CrabTerrainObjectId TerrainObjectId { get; }
        public OsloId OsloId { get; }
        public OsloAssignmentDate AssignmentDate { get; }
        public List<AssignBuildingUnitOsloIdForCrabTerrainObjectId> BuildingUnitOsloIds { get; }

        public AssignOsloIdForCrabTerrainObjectId(
            CrabTerrainObjectId terrainObjectId,
            OsloId osloId,
            OsloAssignmentDate assignmentDate,
            List<AssignBuildingUnitOsloIdForCrabTerrainObjectId> buildingUnitOsloIds)
        {
            TerrainObjectId = terrainObjectId;
            OsloId = osloId;
            AssignmentDate = assignmentDate;
            BuildingUnitOsloIds = buildingUnitOsloIds;
        }

        public Guid CreateCommandId() =>
            Deterministic.Create(Namespace, $"AssignOsloId-{ToString()}");

        public override string ToString() =>
            ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return TerrainObjectId;
            yield return OsloId;
            yield return AssignmentDate;
            yield return BuildingUnitOsloIds;
        }
    }
}
