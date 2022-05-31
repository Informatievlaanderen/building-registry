namespace BuildingRegistry.Legacy
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Newtonsoft.Json;

    public class AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId : ValueObject<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
    {
        public CrabTerrainObjectHouseNumberId CrabTerrainObjectHouseNumberId { get; }
        public CrabSubaddressId CrabSubaddressId { get; }
        public int Index { get; }
        public PersistentLocalId PersistentLocalId { get; }
        public PersistentLocalIdAssignmentDate PersistentLocalIdAssignmentDate { get; }

        public AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
            [JsonProperty("crabTerrainObjectHouseNumberId")] CrabTerrainObjectHouseNumberId crabTerrainObjectHouseNumberId,
            [JsonProperty("crabSubaddressId")] CrabSubaddressId crabSubaddressId,
            [JsonProperty("index")] int index,
            [JsonProperty("persistentLocalId")]  PersistentLocalId persistentLocalId,
            [JsonProperty("persistentLocalIdAssignmentDate")]  PersistentLocalIdAssignmentDate persistentLocalIdAssignmentDate)
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
