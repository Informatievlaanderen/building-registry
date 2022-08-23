namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;

    public class BuildingUnits : List<BuildingUnit>
    {
        private IEnumerable<BuildingUnit> NotRemovedUnits => this.Where(x => !x.IsRemoved);
        public bool DoesNotHaveCommonBuildingUnit => NotRemovedUnits.All(x => x.Function != BuildingUnitFunction.Common);
        public IEnumerable<BuildingUnit> PlannedBuildingUnits => NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Planned);

        public bool RequiresCommonBuildingUnit =>
            DoesNotHaveCommonBuildingUnit
            && NotRemovedUnits.Count(x =>
                x.Status == BuildingUnitStatus.Planned
                || x.Status == BuildingUnitStatus.Realized)
            > 1;

        public bool HasPersistentLocalId(BuildingUnitPersistentLocalId persistentLocalId)
            => this.Any(x => x.BuildingUnitPersistentLocalId == persistentLocalId);
    }
}
