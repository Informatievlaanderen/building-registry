namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingHasInvalidBuildingGeometryMethodException : BuildingRegistryException
    {
        public BuildingHasInvalidBuildingGeometryMethodException()
        { }

        private BuildingHasInvalidBuildingGeometryMethodException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
