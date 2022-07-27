namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingIsRemovedException : BuildingRegistryException
    {
        public BuildingIsRemovedException(int persistentLocalId)
            : base($"Building with Id '{persistentLocalId}' is removed.")
        { }

        private BuildingIsRemovedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
