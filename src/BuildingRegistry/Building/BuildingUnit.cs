namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;

    public class BuildingUnit : Entity
    {
        private IBuildingEvent _lastEvent;
        private List<AddressPersistentLocalId> _addressPersistentLocalIds = new List<AddressPersistentLocalId>();

        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; private set; }
        public BuildingUnitFunction Function { get; private set; }
        public BuildingUnitStatus Status { get; private set; }

        public IReadOnlyList<AddressPersistentLocalId> AddressPersistentLocalIds => _addressPersistentLocalIds;

        public BuildingUnitPosition BuildingUnitPosition { get; private set; }
        public bool IsRemoved { get; private set; }

        public bool HasDeviation { get; private set; }

        public string LastEventHash => _lastEvent.GetHash();
        public ProvenanceData LastProvenanceData => _lastEvent.Provenance;

        public BuildingUnit(Action<object> applier) : base(applier)
        {
            Register<BuildingWasMigrated>(When);

            Register<BuildingUnitWasPlannedV2>(When);
            Register<DeviatedBuildingUnitWasPlanned>(When);
            Register<BuildingUnitWasRealizedV2>(When);
        }

        private void When(BuildingWasMigrated @event)
        {
            _lastEvent = @event;
        }

        private void When(BuildingUnitWasPlannedV2 @event)
        {
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId);
            Function = BuildingUnitFunction.Parse(@event.Function);
            Status = BuildingUnitStatus.Planned;
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));

            HasDeviation = false;

            _lastEvent = @event;
        }

        private void When(DeviatedBuildingUnitWasPlanned @event)
        {
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId);
            Function = BuildingUnitFunction.Parse(@event.Function);
            Status = BuildingUnitStatus.Planned;
            BuildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(@event.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(@event.GeometryMethod));

            HasDeviation = true;

            _lastEvent = @event;
        }

        private void When(BuildingUnitWasRealizedV2 @event)
        {
            Status = BuildingUnitStatus.Realized;

            _lastEvent = @event;
        }


        public static BuildingUnit Migrate(
            Action<object> applier,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitFunction function,
            BuildingUnitStatus status,
            List<AddressPersistentLocalId> addressPersistentLocalIds,
            BuildingUnitPosition buildingUnitPosition,
            bool isRemoved)
        {
            var unit = new BuildingUnit(applier)
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                Function = function,
                Status = status,
                _addressPersistentLocalIds = addressPersistentLocalIds,
                BuildingUnitPosition = buildingUnitPosition,
                IsRemoved = isRemoved,
            };

            return unit;
        }
    }
}
