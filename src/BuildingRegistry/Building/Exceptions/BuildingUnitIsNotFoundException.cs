namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitIsNotFoundException : BuildingRegistryException
    {
        public BuildingUnitIsNotFoundException()
        { }

        private BuildingUnitIsNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public BuildingUnitIsNotFoundException(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId
            ) : base($"BuildingUnit with id '{buildingUnitPersistentLocalId}' was not found in Building '{buildingPersistentLocalId}'.")
        { }
    }
}
