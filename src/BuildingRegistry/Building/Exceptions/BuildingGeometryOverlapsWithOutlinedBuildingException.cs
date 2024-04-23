namespace BuildingRegistry.Building.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class BuildingGeometryOverlapsWithOutlinedBuildingException : BuildingRegistryException
    {
        public BuildingGeometryOverlapsWithOutlinedBuildingException()
        { }

        private BuildingGeometryOverlapsWithOutlinedBuildingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
