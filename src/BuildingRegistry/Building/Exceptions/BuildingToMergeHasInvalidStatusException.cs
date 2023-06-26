namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingToMergeHasInvalidStatusException : BuildingRegistryException
    {
        public BuildingToMergeHasInvalidStatusException()
        { }

        private BuildingToMergeHasInvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
