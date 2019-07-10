namespace BuildingRegistry.ValueObjects
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;

    public class AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId : ValueObject<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
    {
        public CrabTerrainObjectHouseNumberId CrabTerrainObjectHouseNumberId { get; }
        public CrabSubaddressId CrabSubaddressId { get; }
        public int Index { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalIdAssignmentDate PersistentLocalIdAssignmentDate { get; }

        public AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
            CrabTerrainObjectHouseNumberId crabTerrainObjectHouseNumberId,
            CrabSubaddressId crabSubaddressId,
            int index,
            PersistentLocalId persistentLocalId,
            PersistentLocalIdAssignmentDate persistentLocalIdAssignmentDate)
        {
            CrabTerrainObjectHouseNumberId = crabTerrainObjectHouseNumberId;
            CrabSubaddressId = crabSubaddressId;
            Index = index;
            PersistentLocalId = persistentLocalId;
            PersistentLocalIdAssignmentDate = persistentLocalIdAssignmentDate;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return CrabTerrainObjectHouseNumberId;
            yield return CrabSubaddressId;
            yield return Index;
            yield return PersistentLocalId;
            yield return PersistentLocalIdAssignmentDate;
        }
    }
}
