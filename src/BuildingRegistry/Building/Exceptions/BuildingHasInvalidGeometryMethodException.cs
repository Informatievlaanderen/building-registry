namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingHasInvalidGeometryMethodException : BuildingRegistryException
    {
        public BuildingHasInvalidGeometryMethodException()
        { }

        private BuildingHasInvalidGeometryMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
