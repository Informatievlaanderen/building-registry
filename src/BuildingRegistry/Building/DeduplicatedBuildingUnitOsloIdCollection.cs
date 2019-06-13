namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueObjects;

    public class DeduplicatedBuildingUnitOsloIdCollection : HashSet<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
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
          A=1(duplicate A3 because C gets 1 on same T, where 1 was normally preserved for A), 4
          B=2
          C=1
        */

        private readonly Dictionary<AssignBuildingUnitOsloIdForCrabTerrainObjectId, AssignBuildingUnitOsloIdForCrabTerrainObjectId> _duplicatesByOriginal
            = new Dictionary<AssignBuildingUnitOsloIdForCrabTerrainObjectId, AssignBuildingUnitOsloIdForCrabTerrainObjectId>();

        public DeduplicatedBuildingUnitOsloIdCollection(IReadOnlyCollection<AssignBuildingUnitOsloIdForCrabTerrainObjectId> assignBuildingUnitOsloIds)
        {
            foreach (var buildingUnitOsloIdsByIndex in assignBuildingUnitOsloIds.GroupBy(x => x.Index))
            {
                if (buildingUnitOsloIdsByIndex.Count() == 1)
                {
                    if (!_duplicatesByOriginal.Values.Contains(buildingUnitOsloIdsByIndex.First()))
                        Add(buildingUnitOsloIdsByIndex.First());
                }
                else if (buildingUnitOsloIdsByIndex.Count() == 2)
                {
                    foreach (var buildingUnitOsloId in buildingUnitOsloIdsByIndex)
                    {
                        var allByKey = assignBuildingUnitOsloIds
                            .Where(x =>
                                x.CrabTerrainObjectHouseNumberId == buildingUnitOsloId.CrabTerrainObjectHouseNumberId &&
                                x.CrabSubaddressId == buildingUnitOsloId.CrabSubaddressId)
                            .OrderBy(x => x.OsloAssignmentDate)
                            .ToList();

                        if (allByKey.Count == 1)
                        {
                            Add(buildingUnitOsloId);
                        }
                        else
                        {
                            var original = allByKey.First();
                            var duplicate = allByKey.Skip(1).First();
                            if (_duplicatesByOriginal.ContainsKey(original)) //OF value is duplicate ?
                                continue;

                            Add(original);
                            _duplicatesByOriginal.Add(original, duplicate);
                            foreach (var assignBuildingUnitOsloIdForCrabTerrainObjectId in allByKey.Skip(2))
                            {
                                Add(assignBuildingUnitOsloIdForCrabTerrainObjectId);
                            }
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("Deduplication of bug should only have max 2 instances!");
                }
            }

            if (assignBuildingUnitOsloIds.Any())
            {
                // Certain buildings start from index 2 so if 8 - (2 - 1) = 7
                var max = assignBuildingUnitOsloIds.Max(x => x.Index);
                var min = assignBuildingUnitOsloIds.Min(x => x.Index) - 1;

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

        public bool HasDuplicate(AssignBuildingUnitOsloIdForCrabTerrainObjectId assignBuildingUnitOsloId)
        {
            return _duplicatesByOriginal.ContainsKey(assignBuildingUnitOsloId);
        }

        public AssignBuildingUnitOsloIdForCrabTerrainObjectId GetDuplicate(
            AssignBuildingUnitOsloIdForCrabTerrainObjectId originalBuildingUnitOsloId)
        {
            return _duplicatesByOriginal[originalBuildingUnitOsloId];
        }
    }
}
