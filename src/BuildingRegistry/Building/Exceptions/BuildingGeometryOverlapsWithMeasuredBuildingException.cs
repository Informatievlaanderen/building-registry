namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingGeometryOverlapsWithMeasuredBuildingException : BuildingRegistryException
    {
        public BuildingGeometryOverlapsWithMeasuredBuildingException()
        { }

        private BuildingGeometryOverlapsWithMeasuredBuildingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
