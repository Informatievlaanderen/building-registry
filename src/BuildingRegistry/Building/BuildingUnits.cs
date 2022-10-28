namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;

    public class BuildingUnits : List<BuildingUnit>
    {
        private IEnumerable<BuildingUnit> NotRemovedUnits => this.Where(x => !x.IsRemoved);
        public IEnumerable<BuildingUnit> PlannedBuildingUnits => NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Planned);
        public IEnumerable<BuildingUnit> RealizedBuildingUnits => NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Realized);
        public IEnumerable<BuildingUnit> RetiredBuildingUnits => NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Retired);
        public BuildingUnit CommonBuildingUnit => NotRemovedUnits.Single(x => x.Function == BuildingUnitFunction.Common);

        public bool HasCommonBuildingUnit() => NotRemovedUnits.Any(x => x.Function == BuildingUnitFunction.Common);
        public bool HasPlannedOrRealizedCommonBuildingUnit() => NotRemovedUnits.Any(x =>
            x.Function == BuildingUnitFunction.Common
            && (x.Status == BuildingUnitStatus.Planned || x.Status == BuildingUnitStatus.Realized));

        public bool RequiresCommonBuildingUnit() =>
            NotRemovedUnits.Count(x =>
                x.Function != BuildingUnitFunction.Common &&
                (x.Status == BuildingUnitStatus.Planned || x.Status == BuildingUnitStatus.Realized))
            >= 2;

        public bool HasPersistentLocalId(BuildingUnitPersistentLocalId persistentLocalId)
            => this.Any(x => x.BuildingUnitPersistentLocalId == persistentLocalId);
    }
}
