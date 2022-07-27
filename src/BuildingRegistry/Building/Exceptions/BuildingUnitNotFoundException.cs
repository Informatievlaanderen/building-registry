namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitNotFoundException : BuildingRegistryException
    {
        public BuildingUnitNotFoundException()
        { }

        private BuildingUnitNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public BuildingUnitNotFoundException(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId
            ) : base($"BuildingUnit with id '{buildingUnitPersistentLocalId}' was not found in Building '{buildingPersistentLocalId}'.")
        { }
    }
}
