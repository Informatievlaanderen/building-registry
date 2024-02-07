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
            Register<BuildingOutlineWasChanged>(When);
            Register<BuildingWasMeasured>(When);
            Register<BuildingMeasurementWasCorrected>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<CommonBuildingUnitWasAddedV2>(When);
            Register<BuildingUnitWasRealizedV2>(When);
            Register<BuildingUnitWasRealizedBecauseBuildingWasRealized>(When);
            Register<BuildingUnitWasNotRealizedV2>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(When);
            Register<BuildingUnitWasRetiredV2>(When);
            Register<BuildingUnitWasRemovedV2>(When);
            Register<BuildingUnitWasRemovedBecauseBuildingWasRemoved>(When);
            Register<BuildingUnitWasRegularized>(When);
            Register<BuildingUnitWasDeregulated>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(When);
            Register<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRetiredToRealized>(When);
            Register<BuildingUnitRemovalWasCorrected>(When);
            Register<BuildingUnitPositionWasCorrected>(When);
            Register<BuildingUnitRegularizationWasCorrected>(When);
            Register<BuildingUnitDeregulationWasCorrected>(When);
            Register<BuildingUnitAddressWasAttachedV2>(When);
            Register<BuildingUnitAddressWasDetachedV2>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(When);
            Register<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>(When);
            Register<BuildingUnitWasRetiredBecauseBuildingWasDemolished>(When);
            Register<BuildingMeasurementWasChanged>(When);

            // Register<BuildingUnitWasTransferred>(When);
        }

        private void When(BuildingWasMigrated @event)
        {
            _lastEvent = @event;
        }

        private void When(BuildingOutlineWasChanged @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuildingUnits!),
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            _lastEvent = @event;
        }

        private void When(BuildingWasMeasured @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuildingUnits!),
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            _lastEvent = @event;
        }

        private void When(BuildingMeasurementWasCorrected @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuildingUnits!),
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

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

        private void When(BuildingUnitWasRetiredV2 @event)
        {
            Status = BuildingUnitStatus.Retired;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRemovedV2 @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRemovedBecauseBuildingWasRemoved @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRegularized @event)
        {
            HasDeviation = false;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasDeregulated @event)
        {
            HasDeviation = true;

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

        private void When(BuildingUnitWasCorrectedFromNotRealizedToPlanned @event)
        {
            Status = BuildingUnitStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasCorrectedFromRetiredToRealized @event)
        {
            Status = BuildingUnitStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitRemovalWasCorrected @event)
        {
            Status = BuildingUnitStatus.Parse(@event.BuildingUnitStatus);
            Function = BuildingUnitFunction.Parse(@event.Function);
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));
            HasDeviation = @event.HasDeviation;
            _addressPersistentLocalIds.Clear();
            IsRemoved = false;

            _lastEvent = @event;
        }

        private void When(BuildingUnitPositionWasCorrected @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));

            _lastEvent = @event;
        }

        private void When(BuildingUnitRegularizationWasCorrected @event)
        {
            HasDeviation = true;

            _lastEvent = @event;
        }

        private void When(BuildingUnitDeregulationWasCorrected @event)
        {
            HasDeviation = false;

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasAttachedV2 @event)
        {
            _addressPersistentLocalIds.Add(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasDetachedV2 @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRejected @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRetired @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRemoved @event)
        {
            _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.AddressPersistentLocalId));

            _lastEvent = @event;
        }

        private void When(BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed @event)
        {
            if (_addressPersistentLocalIds.Contains(new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId)))
            {
                _addressPersistentLocalIds.Remove(new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId));
            }

            if (!_addressPersistentLocalIds.Contains(new AddressPersistentLocalId(@event.NewAddressPersistentLocalId)))
            {
                _addressPersistentLocalIds.Add(new AddressPersistentLocalId(@event.NewAddressPersistentLocalId));
            }

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasNotRealizedBecauseBuildingWasDemolished @event)
        {
            Status = BuildingUnitStatus.NotRealized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRetiredBecauseBuildingWasDemolished @event)
        {
            Status = BuildingUnitStatus.Retired;

            _lastEvent = @event;
        }

        private void When(BuildingMeasurementWasChanged @event)
        {
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuildingUnits),
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            _lastEvent = @event;
        }

        // private void When(BuildingUnitWasTransferred @event)
        // {
        //     _lastEvent = @event;
        // }
    }
}
