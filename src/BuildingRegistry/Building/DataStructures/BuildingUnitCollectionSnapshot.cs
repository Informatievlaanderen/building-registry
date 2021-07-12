namespace BuildingRegistry.Building.DataStructures
{
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public class BuildingUnitCollectionSnapshot
    {
        public IEnumerable<BuildingUnitSnapshot> BuildingUnits { get; }
        public Dictionary<BuildingUnitKeyType, BuildingUnitKeyType> ReaddressedKeys { get; }

        public BuildingUnitCollectionSnapshot(
            IEnumerable<BuildingUnitSnapshot> buildingUnits,
            Dictionary<BuildingUnitKey, BuildingUnitKey> readdressedKeys)
        {
            BuildingUnits = buildingUnits;
            ReaddressedKeys = readdressedKeys.ToDictionary(
                x => (BuildingUnitKeyType)x.Key,
                y => (BuildingUnitKeyType)y.Value);
        }
    }
}
