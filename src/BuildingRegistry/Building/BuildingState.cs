namespace BuildingRegistry.Building
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Events;
    using Exceptions;

    public partial class Building
    {
        private IBuildingEvent _lastEvent;
        private readonly List<BuildingUnit> _buildingUnits = new List<BuildingUnit>();

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; private set; }
        public BuildingStatus BuildingStatus { get; private set; }
        public BuildingGeometry BuildingGeometry { get; private set; }
        public bool IsRemoved { get; private set; }

        public IReadOnlyList<BuildingUnit> BuildingUnits => _buildingUnits;

        public string LastEventHash => _lastEvent.GetHash();

        internal Building(ISnapshotStrategy snapshotStrategy)
            : this()
        {
            Strategy = snapshotStrategy;
        }

        private Building()
        {
            Register<BuildingWasMigrated>(When);
            Register<BuildingWasPlannedV2>(When);
            Register<BuildingBecameUnderConstructionV2>(When);
            Register<BuildingWasRealizedV2>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<DeviatedBuildingUnitWasPlanned>(When);
            Register<BuildingUnitWasRealizedV2>(When);
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
                    new BuildingUnitPersistentLocalId(buildingUnit.BuildingUnitPersistentLocalId),
                    BuildingUnitFunction.Parse(buildingUnit.Function),
                    BuildingUnitStatus.Parse(buildingUnit.Status),
                    buildingUnit.AddressPersistentLocalIds.ConvertAll(x => new AddressPersistentLocalId(x)),
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
            BuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            BuildingStatus = BuildingStatus.Planned;

            _lastEvent = @event;
        }

        private void When(BuildingBecameUnderConstructionV2 @event)
        {
            BuildingStatus = BuildingStatus.UnderConstruction;

            _lastEvent = @event;
        }
        private void When(BuildingWasRealizedV2 @event)
        {
            BuildingStatus = BuildingStatus.Realized;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasPlannedV2 @event)
        {
            var newBuildingUnit = new BuildingUnit(ApplyChange);
            newBuildingUnit.Route(@event);
            _buildingUnits.Add(newBuildingUnit);

            _lastEvent = @event;
        }

        private void When(DeviatedBuildingUnitWasPlanned @event)
        {
            var newBuildingUnit = new BuildingUnit(ApplyChange);
            newBuildingUnit.Route(@event);
            _buildingUnits.Add(newBuildingUnit);

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event)
        {
            var buildingUnit = BuildingUnits.FirstOrDefault(x => x.BuildingUnitPersistentLocalId == @event.BuildingUnitPersistentLocalId);

            if (buildingUnit is null)
            {
                throw new BuildingUnitNotFoundException(
                    BuildingPersistentLocalId,
                    @event.BuildingUnitPersistentLocalId);
            }

            buildingUnit.Route(@event);

            _lastEvent = @event;
        }
    }
}
