namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;

    public class BuildingUnits : List<BuildingUnit>
    {
        private IEnumerable<BuildingUnit> NotRemovedUnits => this.Where(x => !x.IsRemoved);

        public IEnumerable<BuildingUnit> NonCommonBuildingUnits(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Where(x => x.Function != BuildingUnitFunction.Common)
                : this.Where(x => x.Function != BuildingUnitFunction.Common);
        public IEnumerable<BuildingUnit> PlannedBuildingUnits(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Planned)
                : this.Where(x => x.Status == BuildingUnitStatus.Planned);
        public IEnumerable<BuildingUnit> RealizedBuildingUnits(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Realized)
                : this.Where(x => x.Status == BuildingUnitStatus.Realized);
        public IEnumerable<BuildingUnit> RetiredBuildingUnits(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Where(x => x.Status == BuildingUnitStatus.Retired)
                : this.Where(x => x.Status == BuildingUnitStatus.Retired);

        public BuildingUnit CommonBuildingUnit(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Single(x => x.Function == BuildingUnitFunction.Common)
                : this.Single(x => x.Function == BuildingUnitFunction.Common);

        public bool HasCommonBuildingUnit(bool excludeRemoved = true)
            => excludeRemoved
                ? NotRemovedUnits.Any(x => x.Function == BuildingUnitFunction.Common)
                : this.Any(x => x.Function == BuildingUnitFunction.Common);

        public bool HasPlannedOrRealizedCommonBuildingUnit(bool excludeRemoved = true)
        {
            var set = excludeRemoved ? NotRemovedUnits : this;

            return set.Any(x =>
                x.Function == BuildingUnitFunction.Common
                && (x.Status == BuildingUnitStatus.Planned || x.Status == BuildingUnitStatus.Realized));
        }

        public bool RequiresCommonBuildingUnit() =>
            NotRemovedUnits.Count(x =>
                x.Function != BuildingUnitFunction.Common &&
                (x.Status == BuildingUnitStatus.Planned || x.Status == BuildingUnitStatus.Realized))
            >= 2;

        public bool HasPersistentLocalId(BuildingUnitPersistentLocalId persistentLocalId)
            => this.Any(x => x.BuildingUnitPersistentLocalId == persistentLocalId);
    }
}
