namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public class DeduplicatedBuildingUnitPersistentLocalIdCollection : HashSet<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
    {
        /*
         GE + Index = unique id
         GE | Index | T
         ----------------
          A |   1   | T1
          B |   2   | T2
          C |   1   | T4
          A |   3   | T4
          A |   4   | T5

          Indexes for each GE
          A=1 (duplicate A3 because C gets 1 on same T, where 1 was normally preserved for A), 4
          B=2
          C=1
        */

        private readonly Dictionary<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId, AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId> _duplicatesByOriginal
            = new Dictionary<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId, AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>();

        public DeduplicatedBuildingUnitPersistentLocalIdCollection(IReadOnlyCollection<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId> assignBuildingUnitPersistentLocalIds)
        {
            foreach (var buildingUnitPersistentLocalIdsByIndex in assignBuildingUnitPersistentLocalIds.GroupBy(x => x.Index))
            {
                if (buildingUnitPersistentLocalIdsByIndex.Count() == 1)
                {
                    if (!_duplicatesByOriginal.Values.Contains(buildingUnitPersistentLocalIdsByIndex.First()))
                        Add(buildingUnitPersistentLocalIdsByIndex.First());
                }
                else if (buildingUnitPersistentLocalIdsByIndex.Count() == 2)
                {
                    foreach (var buildingUnitPersistentLocalId in buildingUnitPersistentLocalIdsByIndex)
                    {
                        var allByKey = assignBuildingUnitPersistentLocalIds
                            .Where(x =>
                                x.CrabTerrainObjectHouseNumberId == buildingUnitPersistentLocalId.CrabTerrainObjectHouseNumberId &&
                                x.CrabSubaddressId == buildingUnitPersistentLocalId.CrabSubaddressId)
                            .OrderBy(x => x.PersistentLocalIdAssignmentDate)
                            .ToList();

                        if (allByKey.Count == 1)
                        {
                            Add(buildingUnitPersistentLocalId);
                        }
                        else
                        {
                            var original = allByKey.First();
                            var duplicate = allByKey.Skip(1).First();
                            if (_duplicatesByOriginal.ContainsKey(original)) //OF value is duplicate ?
                                continue;

                            Add(original);
                            _duplicatesByOriginal.Add(original, duplicate);
                            foreach (var assignBuildingUnitPersistentLocalIdForCrabTerrainObjectId in allByKey.Skip(2))
                                Add(assignBuildingUnitPersistentLocalIdForCrabTerrainObjectId);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Deduplication of bug should only have max 2 instances!");
                }
            }

            if (assignBuildingUnitPersistentLocalIds.Any())
            {
                // Certain buildings start from index 2 so if 8 - (2 - 1) = 7
                var max = assignBuildingUnitPersistentLocalIds.Max(x => x.Index);
                var min = assignBuildingUnitPersistentLocalIds.Min(x => x.Index) - 1;

                if ((max - min) != Count)
                {
                    /*
                     * If you have a indexes as follows: 1,3,2,3,4
                     * Deduplicated would be: 1,2,3,4
                     * 4 items == max index = 4
                     */
                    throw new InvalidOperationException("Deduplicates should equal max index (distinct)");
                }
            }
        }

        public bool HasDuplicate(AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId assignBuildingUnitPersistentLocalId)
        {
            return _duplicatesByOriginal.ContainsKey(assignBuildingUnitPersistentLocalId);
        }

        public AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId GetDuplicate(
            AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId originalBuildingUnitPersistentLocalId)
        {
            return _duplicatesByOriginal[originalBuildingUnitPersistentLocalId];
        }
    }
}
