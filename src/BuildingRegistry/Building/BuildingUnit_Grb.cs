namespace BuildingRegistry.Building
{
    using System.Linq;
    using Events;

    public sealed partial class BuildingUnit
    {
        public void Demolish()
        {
            if (IsRemoved)
            {
                return;
            }

            if (Status == BuildingUnitStatus.NotRealized || Status == BuildingUnitStatus.Retired)
            {
                return;
            }

            foreach (var addressPersistentLocalId in _addressPersistentLocalIds.ToList())
            {
                Apply(new BuildingUnitAddressWasDetachedV2(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId,
                    addressPersistentLocalId));
            }

            if (Status == BuildingUnitStatus.Planned)
            {
                Apply(new BuildingUnitWasNotRealizedBecauseBuildingWasDemolished(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId));
            }
            else if (Status == BuildingUnitStatus.Realized)
            {
                Apply(new BuildingUnitWasRetiredBecauseBuildingWasDemolished(
                    _buildingPersistentLocalId,
                    BuildingUnitPersistentLocalId));
            }
        }
    }
}
