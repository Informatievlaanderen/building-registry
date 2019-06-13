namespace BuildingRegistry.ValueObjects
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;

    public class AssignBuildingUnitOsloIdForCrabTerrainObjectId : ValueObject<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
    {
        public CrabTerrainObjectHouseNumberId CrabTerrainObjectHouseNumberId { get; }
        public CrabSubaddressId CrabSubaddressId { get; }
        public int Index { get; }
        public OsloId OsloId { get; }
        public OsloAssignmentDate OsloAssignmentDate { get; }

        public AssignBuildingUnitOsloIdForCrabTerrainObjectId(
            CrabTerrainObjectHouseNumberId crabTerrainObjectHouseNumberId,
            CrabSubaddressId crabSubaddressId,
            int index,
            OsloId osloId,
            OsloAssignmentDate osloAssignmentDate)
        {
            CrabTerrainObjectHouseNumberId = crabTerrainObjectHouseNumberId;
            CrabSubaddressId = crabSubaddressId;
            Index = index;
            OsloId = osloId;
            OsloAssignmentDate = osloAssignmentDate;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return CrabTerrainObjectHouseNumberId;
            yield return CrabSubaddressId;
            yield return Index;
            yield return OsloId;
            yield return OsloAssignmentDate;
        }
    }
}
