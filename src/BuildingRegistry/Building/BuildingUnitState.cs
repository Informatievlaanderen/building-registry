namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class BuildingUnit
    {
        private IBuildingEvent? _lastEvent;
        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds = new List<AddressPersistentLocalId>();
        private BuildingPersistentLocalId _buildingPersistentLocalId;

        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; private set; }
        public BuildingUnitFunction Function { get; private set; }
        public BuildingUnitStatus Status { get; private set; }

        public IReadOnlyList<AddressPersistentLocalId> AddressPersistentLocalIds => _addressPersistentLocalIds;

        public BuildingUnitPosition BuildingUnitPosition { get; private set; }
        public bool IsRemoved { get; private set; }

        public bool HasDeviation { get; private set; }

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        public BuildingUnit(Action<object> applier) : base(applier)
        {
            Register<BuildingWasMigrated>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<BuildingUnitWasRealizedV2>(When);
        }

        private void When(BuildingWasMigrated @event)
        {
            _lastEvent = @event;
        }

        private void When(BuildingUnitWasPlannedV2 @event)
        {
            _buildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId);
            Function = BuildingUnitFunction.Parse(@event.Function);
            Status = BuildingUnitStatus.Planned;
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));

            HasDeviation = @event.HasDeviation;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event)
        {
            Status = BuildingUnitStatus.Realized;

            _lastEvent = @event;
        }
    }
}
