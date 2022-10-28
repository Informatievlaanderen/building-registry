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
            Register<BuildingOutlineWasChanged>(When);
            Register<BuildingUnitWasRealizedV2>(When);
            Register<BuildingUnitWasRealizedBecauseBuildingWasRealized>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(When);
            Register<BuildingUnitWasNotRealizedV2>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(When);
            Register<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingUnitWasRetiredV2>(When);
            Register<CommonBuildingUnitWasAddedV2>(When);
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

        private void When(BuildingOutlineWasChanged @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuildingUnits!),
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event)
        {
            Status = BuildingUnitStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedBecauseBuildingWasRealized @event)
        {
            Status = BuildingUnitStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasCorrectedFromRealizedToPlanned @event)
        {
            Status = BuildingUnitStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected @event)
        {
            Status = BuildingUnitStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasNotRealizedV2 @event)
        {
            Status = BuildingUnitStatus.NotRealized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized @event)
        {
            Status = BuildingUnitStatus.NotRealized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasCorrectedFromNotRealizedToPlanned @event)
        {
            Status = BuildingUnitStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRetiredV2 @event)
        {
            Status = BuildingUnitStatus.Retired;

            _lastEvent = @event;
        }

        private void When(CommonBuildingUnitWasAddedV2 @event)
        {
            _buildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId);
            Function = BuildingUnitFunction.Common;
            Status = BuildingUnitStatus.Parse(@event.BuildingUnitStatus);
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));

            HasDeviation = @event.HasDeviation;

            _lastEvent = @event;
        }
    }
}
