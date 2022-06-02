namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;

    public class BuildingUnit : Entity
    {
        private IHaveHash _lastEvent;

        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; private set; }
        public BuildingUnitFunction Function { get; private set; }
        public BuildingUnitStatus Status { get; private set; }
        public List<AddressPersistentLocalId> AddressPersistentLocalIds { get; private set; }
        public BuildingUnitPosition BuildingUnitPosition { get; private set; }
        public bool IsRemoved { get; private set; }

        public BuildingUnit(Action<object> applier) : base(applier)
        {

        }

        public static BuildingUnit Migrate(
            Action<object> applier,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitFunction function,
            BuildingUnitStatus status,
            List<AddressPersistentLocalId> addressPersistentLocalIds,
            BuildingUnitPosition buildingUnitPosition, bool isRemoved)
        {
            var unit = new BuildingUnit(applier);

            unit.BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            unit.Function = function;
            unit.Status = status;
            unit.AddressPersistentLocalIds = addressPersistentLocalIds;
            unit.BuildingUnitPosition = buildingUnitPosition;
            unit.IsRemoved = isRemoved;

            return unit;
        }
    }
}
