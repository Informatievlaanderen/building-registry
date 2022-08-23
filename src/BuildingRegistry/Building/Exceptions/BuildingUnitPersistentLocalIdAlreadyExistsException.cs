namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingUnitPersistentLocalIdAlreadyExistsException : BuildingRegistryException
    {
        public BuildingUnitPersistentLocalIdAlreadyExistsException()
        { }

        private BuildingUnitPersistentLocalIdAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
