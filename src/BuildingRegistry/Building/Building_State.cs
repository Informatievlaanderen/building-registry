namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public partial class Building
    {
        private readonly IAddresses _addresses;
        private readonly IAddCommonBuildingUnit _addCommonBuildingUnit;
        private IBuildingEvent? _lastEvent;

        private string _lastSnapshotEventHash = string.Empty;
        private ProvenanceData _lastSnapshotProvenance;

        private readonly BuildingUnits _buildingUnits = new BuildingUnits();

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; private set; }
        public BuildingStatus BuildingStatus { get; private set; }
        public BuildingGeometry BuildingGeometry { get; private set; }
        public bool IsRemoved { get; private set; }

        public IReadOnlyList<BuildingUnit> BuildingUnits => _buildingUnits;

        public string LastEventHash => _lastEvent is null ? _lastSnapshotEventHash : _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData =>
            _lastEvent is null ? _lastSnapshotProvenance : _lastEvent.Provenance;

        internal Building(
            ISnapshotStrategy snapshotStrategy,
            IAddCommonBuildingUnit addCommonBuildingUnit,
            IAddresses addresses)
            : this()
        {
            Strategy = snapshotStrategy;
            _addCommonBuildingUnit = addCommonBuildingUnit;
            _addresses = addresses;
        }

        private Building()
        {
            Register<BuildingWasMigrated>(When);
            Register<BuildingWasPlannedV2>(When);
            Register<BuildingOutlineWasChanged>(When);
            Register<BuildingBecameUnderConstructionV2>(When);
            Register<BuildingWasCorrectedFromUnderConstructionToPlanned>(When);
            Register<BuildingWasRealizedV2>(When);
            Register<BuildingWasCorrectedFromRealizedToUnderConstruction>(When);
            Register<BuildingWasNotRealizedV2>(When);
            Register<BuildingWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingWasRemovedV2>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<BuildingUnitWasRealizedV2>(When);
            Register<BuildingUnitWasRealizedBecauseBuildingWasRealized>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlanned>(When);
            Register<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(When);
            Register<BuildingUnitWasNotRealizedV2>(When);
            Register<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(When);
            Register<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(When);
            Register<BuildingUnitWasRetiredV2>(When);
            Register<BuildingUnitWasCorrectedFromRetiredToRealized>(When);
            Register<BuildingUnitPositionWasCorrected>(When);
            Register<BuildingUnitWasRemovedV2>(When);
            Register<BuildingUnitWasRemovedBecauseBuildingWasRemoved>(When);
            Register<BuildingUnitRemovalWasCorrected>(When);
            Register<BuildingUnitWasRegularized>(When);
            Register<BuildingUnitRegularizationWasCorrected>(When);
            Register<BuildingUnitWasDeregulated>(When);
            Register<BuildingUnitDeregulationWasCorrected>(When);
            Register<BuildingUnitFunctionWasChanged>(When);
            Register<CommonBuildingUnitWasAddedV2>(When);
            Register<BuildingUnitAddressWasAttachedV2>(When);
            Register<BuildingUnitAddressWasDetachedV2>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(When);
            Register<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(When);

            Register<BuildingSnapshot>(When);
        }

        private void When(BuildingWasMigrated @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingStatus = BuildingStatus.Parse(@event.BuildingStatus);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Parse(@event.GeometryMethod));
            IsRemoved = @event.IsRemoved;

            foreach (var buildingUnit in @event.BuildingUnits)
            {
                var newBuildingUnit = BuildingUnit.Migrate(
                    ApplyChange,
                    BuildingPersistentLocalId,
                    new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
                    BuildingUnitFunction.Parse(buildingUnit.Function),
                    BuildingUnitStatus.Parse(buildingUnit.Status),
                    buildingUnit.AddressPersistentLocalIds
                        .Distinct()
                        .ToList()
                        .ConvertAll(x => new AddressPersistentLocalId(x)),
                    new BuildingUnitPosition(
                        new ExtendedWkbGeometry(buildingUnit.ExtendedWkbGeometry),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod)),
                    buildingUnit.IsRemoved);

                newBuildingUnit.Route(@event);
                _buildingUnits.Add(newBuildingUnit);
            }

            _lastEvent = @event;
        }

        private void When(BuildingWasPlannedV2 @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingOutlineWasChanged @event)
        {
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometryBuilding),
                BuildingGeometryMethod.Outlined);

            foreach (var buildingUnitPersistentLocalId in @event.BuildingUnitPersistentLocalIds)
            {
                var buildingUnit = BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                buildingUnit.Route(@event);
            }

            _lastEvent = @event;
        }

        private void When(BuildingBecameUnderConstructionV2 @event)
        {
            BuildingStatus = BuildingStatus.UnderConstruction;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromUnderConstructionToPlanned @event)
        {
            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingWasRealizedV2 @event)
        {
            BuildingStatus = BuildingStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromRealizedToUnderConstruction @event)
        {
            BuildingStatus = BuildingStatus.UnderConstruction;

            _lastEvent = @event;
        }

        private void When(BuildingWasNotRealizedV2 @event)
        {
            BuildingStatus = BuildingStatus.NotRealized;

            _lastEvent = @event;
        }

        private void When(BuildingWasCorrectedFromNotRealizedToPlanned @event)
        {
            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingWasRemovedV2 @event)
        {
            IsRemoved = true;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasPlannedV2 @event)
        {
            var newBuildingUnit = new BuildingUnit(ApplyChange);
            newBuildingUnit.Route(@event);
            _buildingUnits.Add(newBuildingUnit);

            _lastEvent = @event;
        }

        private void When(CommonBuildingUnitWasAddedV2 @event)
        {
            var commonBuildingUnit = new BuildingUnit(ApplyChange);
            commonBuildingUnit.Route(@event);
            _buildingUnits.Add(commonBuildingUnit);

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRealizedBecauseBuildingWasRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRealizedToPlanned @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasNotRealizedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromNotRealizedToPlanned @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRetiredV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasCorrectedFromRetiredToRealized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitPositionWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRemovedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRemovedBecauseBuildingWasRemoved @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitRemovalWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasRegularized @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitRegularizationWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitWasDeregulated @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitDeregulationWasCorrected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitFunctionWasChanged @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasAttachedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedV2 @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRemoved @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRejected @event) => RouteToBuildingUnit(@event);

        private void When(BuildingUnitAddressWasDetachedBecauseAddressWasRetired @event) => RouteToBuildingUnit(@event);

        private void RouteToBuildingUnit<TEvent>(TEvent @event)
            where TEvent : IBuildingEvent, IHasBuildingUnitPersistentLocalId
        {
            _buildingUnits
                .GetByPersistentLocalId(new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId))
                .Route(@event);
        }

        private void When(BuildingSnapshot @event)
        {
            BuildingPersistentLocalId = new BuildingPersistentLocalId(@event.BuildingPersistentLocalId);
            BuildingStatus = BuildingStatus.Parse(@event.BuildingStatus);
            BuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Parse(@event.GeometryMethod));
            IsRemoved = @event.IsRemoved;

            foreach (var buildingUnitData in @event.BuildingUnits)
            {
                var buildingUnit = new BuildingUnit(ApplyChange);
                buildingUnit.RestoreSnapshot(BuildingPersistentLocalId, buildingUnitData);

                _buildingUnits.Add(buildingUnit);
            }

            _lastSnapshotEventHash = @event.LastEventHash;
            _lastSnapshotProvenance = @event.LastProvenanceData;
        }
    }
}
