namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingToMergeHasInvalidGeometryMethodException : BuildingRegistryException
    {
        public BuildingToMergeHasInvalidGeometryMethodException()
        { }

        private BuildingToMergeHasInvalidGeometryMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
