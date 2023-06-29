namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingToMergeNotFoundException : BuildingRegistryException
    {
        public int BuildingPersistentLocalId { get; set; }

        public BuildingToMergeNotFoundException()
        { }

        public BuildingToMergeNotFoundException(int persistentLocalId)
            : base($"Building with Id '{persistentLocalId}' does not exist.")
        {
            BuildingPersistentLocalId = persistentLocalId;
        }

        private BuildingToMergeNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
