namespace BuildingRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Building;
    using Building.Datastructures;

    /// <summary>
    /// The migrator only ever dispatches TransformToLambert2008, but the command handler resolver
    /// constructs every registered command handler module, so BuildingUnitCommandHandlerModule still has
    /// to resolve. These stand in for the dependencies only its other handlers use, which keeps the
    /// migrator free of the BackOffice and Consumer.Address databases. They throw rather than no-op:
    /// reaching one means a command was dispatched that this migrator has no business dispatching.
    /// </summary>
    internal sealed class UnreachableAddCommonBuildingUnit : IAddCommonBuildingUnit
    {
        public int GenerateNextPersistentLocalId()
            => throw new NotSupportedException(
                "The Lambert 2008 migrator does not create building units.");

        public void AddForBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
            => throw new NotSupportedException(
                "The Lambert 2008 migrator does not create building units.");
    }

    /// <inheritdoc cref="UnreachableAddCommonBuildingUnit"/>
    internal sealed class UnreachableAddresses : IAddresses
    {
        public AddressData? GetOptional(AddressPersistentLocalId addressPersistentLocalId)
            => throw new NotSupportedException(
                "The Lambert 2008 migrator does not read addresses.");

        public Task<List<AddressData>> GetAddresses(List<AddressPersistentLocalId> addressPersistentLocalIds)
            => throw new NotSupportedException(
                "The Lambert 2008 migrator does not read addresses.");
    }
}
