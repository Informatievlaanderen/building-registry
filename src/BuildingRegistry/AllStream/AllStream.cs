namespace BuildingRegistry.AllStream
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using Events;

    public sealed class AllStream : AggregateRootEntity
    {
        public void CreateOsloSnapshots(IReadOnlyList<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds)
        {
            ApplyChange(new BuildingUnitOsloSnapshotsWereRequested(buildingUnitPersistentLocalIds));
        }
    }
}
